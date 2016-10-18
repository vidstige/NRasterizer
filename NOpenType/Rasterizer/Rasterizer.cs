using System;
using System.Collections.Generic;
using System.Text;

namespace NRasterizer.Rasterizer
{
    public class Rasterizer: IGlyphRasterizer
    {
        private readonly Typeface _typeface;
        private const int pointsPerInch = 72;

        public Rasterizer(Typeface typeface, Raster target)
        {
            _typeface = typeface;
            _target = target;
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
