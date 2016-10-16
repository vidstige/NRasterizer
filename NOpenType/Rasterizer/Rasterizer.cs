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

        public static IEnumerable<Point> InsertImplicit(IEnumerable<Point> points)
        {
            throw new System.NotSupportedException();
            //var previous = points.First();
            //yield return previous;
            //foreach (var p in points.Skip(1))
            //{
            //    if (!previous.On && !p.On)
            //    {
            //        // implicit point on curve
            //        yield return new Point((short)((previous.X + p.X) / 2), (short)((previous.Y + p.Y) / 2), true);
            //    }
            //    previous = p;
            //    yield return p;
            //}
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

    public class Rasterizer
    {
        private readonly Typeface _typeface;
        private const int pointsPerInch = 72;

        public Rasterizer(Typeface typeface)
        {
            _typeface = typeface;
        }

        private void SetScanFlags(Glyph glyph, Raster scanFlags, int fx, int fy, int size, int x, int y)
        {
            float scale = (float)(size * scanFlags.Resolution) / (pointsPerInch * _typeface.UnitsPerEm);
            var pixels = scanFlags.Pixels;
            for (int contour = 0; contour < glyph.EndPoints.Length; contour++)
            {
                var aerg = new List<Segment>(GlyphHelpers.GetContourIterator(glyph, contour, fx, fy, x, y, scale, -scale));
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

        public void Rasterize(string text, int size, Raster raster, bool toFlags = false)
        {
            var flags = new Raster(raster.Width, raster.Height, raster.Stride, raster.Resolution);

            // 
            int fx = 64;
            int fy = 0;
            foreach (var character in text)
            {
                var glyph = _typeface.Lookup(character);
                SetScanFlags(glyph, flags, fx, fy, size, 0, 70);
                fx += _typeface.GetAdvanceWidth(character);
            }

            if (toFlags)
            {
                RenderFlags(flags, raster);
            }
            else
            {
                RenderScanlines(flags, raster);
            }
        }

        // TODO: Duplicated code from Rasterize & SetScanFlags
        public IEnumerable<Segment> GetAllSegments(string text, int size, int resolution)
        {
            int x = 0;
            int y = 70;

            // 
            int fx = 64;
            int fy = 0;
            foreach (var character in text)
            {
                var glyph = _typeface.Lookup(character);

                float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);
                for (int contour = 0; contour < glyph.EndPoints.Length; contour++)
                {
                    var aerg = new List<Segment>(GlyphHelpers.GetContourIterator(glyph, contour, fx, fy, x, y, scale, -scale));
                    foreach (var segment in GlyphHelpers.GetContourIterator(glyph, contour, fx, fy, x, y, scale, -scale))
                    {
                        yield return segment;
                    }
                }

                fx += _typeface.GetAdvanceWidth(character);
            }
        }
    }
}
