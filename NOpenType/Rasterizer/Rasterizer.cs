using System;
using System.Collections.Generic;
using System.Text;

namespace NRasterizer.Rasterizer
{
    public static class GlyphHelpers
    {
        public static int ContourCount(Glyph glyph) { return glyph.EndPoints.Length; }

        public static Point At(Glyph glyph, int pointIndex)
        {
            return new Point(glyph.X[pointIndex], glyph.Y[pointIndex], glyph.On[pointIndex]);
        }

        public static IEnumerable<Point> GetContourPoints(Glyph glyph, int contourIndex)
        {
            var begin = GetContourBegin(glyph, contourIndex);
            var end = GetContourEnd(glyph, contourIndex);
            for (int i = begin; i <= end; i++)
            {
                yield return At(glyph, i);
            }
        }

        public static List<Point> InsertImplicit2(IEnumerable<Point> points)
        {
            List<Point> newPointList = new List<Point>();
            Point previous = null;
            bool isFirstPoint = true;
            foreach (Point p in points)
            {
                if (isFirstPoint)
                {
                    newPointList.Add(previous = p);
                    isFirstPoint = false;
                }
                else
                {
                    //others
                    if (!previous.On && !p.On)
                    {
                        newPointList.Add(new Point((short)((previous.X + p.X) / 2), (short)((previous.Y + p.Y) / 2), true));
                    }
                    previous = p;
                    newPointList.Add(p);
                }
            }

            return newPointList;
        }

        public static  T Circular<T>(List<T> list, int index)
        {
            return list[index % list.Count];
        }

        public static IEnumerable<Segment> GetContourIterator(Glyph glyph, int contourIndex,
            int fontX, int fontY,
            float xOffset, float yOffset, float scaleX, float scaleY)
        {
            var pts = InsertImplicit2(GetContourPoints(glyph, contourIndex));

            var begin = GetContourBegin(glyph, contourIndex);
            var end = GetContourEnd(glyph, contourIndex);

            var _x = glyph.X;
            var _y = glyph.Y;
            for (int i = 0; i < end - begin; i += pts[(i + 1) % pts.Count].On ? 1 : 2)
            {
                if (pts[(i + 1) % pts.Count].On)
                {
                    yield return new Line(
                        (int)(xOffset + (fontX + pts[i].X) * scaleX),
                        (int)(yOffset + (fontY + pts[i].Y) * scaleY),
                        (int)(xOffset + (fontX + Circular(pts, i + 1).X) * scaleX),
                        (int)(yOffset + (fontY + Circular(pts, i + 1).Y) * scaleY));
                }
                else
                {
                    yield return new Bezier(
                        xOffset + (fontX + pts[i].X) * scaleX,
                        yOffset + (fontY + pts[i].Y) * scaleY,
                        xOffset + (fontX + Circular(pts, i + 1).X) * scaleX,
                        yOffset + (fontY + Circular(pts, i + 1).Y) * scaleY,
                        xOffset + (fontX + Circular(pts, i + 2).X) * scaleX,
                        yOffset + (fontY + Circular(pts, i + 2).Y) * scaleY);
                }
            }
            // TODO: What if the last segment if a bezier
            yield return new Line(
                (int)(xOffset + (fontX + _x[end]) * scaleX),
                (int)(yOffset + (fontY + _y[end]) * scaleY),
                (int)(xOffset + (fontX + _x[begin]) * scaleX),
                (int)(yOffset + (fontY + _y[begin]) * scaleY));
        }

        public static int GetContourBegin(Glyph glyph, int contourIndex)
        {
            if (contourIndex == 0) return 0;
            return glyph.EndPoints[contourIndex - 1] + 1;
        }        

        private static int GetContourEnd(Glyph glyph, int contourIndex)
        {
            return glyph.EndPoints[contourIndex];
        }

    }

    public class Rasterizer: IGlyphRasterizer
    {
        private readonly Typeface _typeface;
        private const int pointsPerInch = 72;

        public Rasterizer(Typeface typeface, Raster target)
        {
            _typeface = typeface;
            _target = target;
        }

        private void SetScanFlags(Glyph glyph, Raster scanFlags, int fx, int fy, int size, int x, int y)
        {
            float scale = (float)(size * scanFlags.Resolution) / (pointsPerInch * _typeface.UnitsPerEm);
            var pixels = scanFlags.Pixels;
            for (int contour = 0; contour < glyph.EndPoints.Length; contour++)
            {
                foreach (var segment in GlyphHelpers.GetContourIterator(glyph, contour, fx, fy, x, y, scale, -scale))
                {
                    segment.FillFlags(scanFlags);
                }
            }
        }

        private void RenderFlags(Raster scanFlags, Raster target)
        {
            var source = scanFlags.Pixels;
            var destinataion = target.Pixels;
            var stride = target.Stride;
            for (int y = 0; y < target.Height; y++)
            {
                int row = stride * y;
                for (int x = 0; x < target.Width; x++)
                {
                    destinataion[row + x] = (byte)(source[row + x] * 128);
                }
            }
        }

        private void RenderScanlines(Raster scanFlags, Raster target)
        {
            var source = scanFlags.Pixels;
            var destinataion = target.Pixels;
            var stride = target.Stride;

            for (int y = 0; y < target.Height; y++)
            {
                bool fill = false;
                int row = stride * y;
                for (int x = 0; x < target.Width; x++)
                {
                    if (source[row + x] > 0)
                    {
                        fill = !fill;
                    }
                    destinataion[row + x] = fill ? (byte)255 : (byte)0;
                }
            }
        }

        #region IGlyphRasterizer implementation

        private readonly Raster _target;
        private Raster _flags;
        private double _x;
        private double _y;

        public void BeginRead(int countourCount)
        {
            _flags = new Raster(_target.Width, _target.Height, _target.Stride, _target.Resolution);
        }

        public void EndRead()
        {
            RenderScanlines(_flags, _target);
        }

        public void LineTo(double x, double y)
        {
            new Line((int)_x, (int)_y, (int)x, (int)y).FillFlags(_flags);
        }

        public void Curve3(double p2x, double p2y, double x, double y)
        {
            new Bezier((int)_x, (int)_y, (int)p2x, (int)p2y, (int)x, (int)y).FillFlags(_flags);
        }

        public void Curve4(double p2x, double p2y, double p3x, double p3y, double x, double y)
        {
            // TODO: subdivide...
        }

        public void MoveTo(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public void CloseFigure()
        {
        }

        #endregion
    }
}
