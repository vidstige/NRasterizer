using System;
using System.Collections.Generic;
using System.Text;

namespace NRasterizer.Rasterizer
{
    public class Rasterizer: IGlyphRasterizer
    {
        private const int pointsPerInch = 72;

        public Rasterizer(Raster target)
        {
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

        // Special hack
        private bool first;
        private double _firstX;
        private double _firstY;

        public void BeginRead(int countourCount)
        {
            _flags = new Raster(_target.Width, _target.Height, _target.Stride, _target.Resolution);
            first = true;
        }

        public void EndRead()
        {
            RenderScanlines(_flags, _target);
            //RenderFlags(_flags, _target);
        }

        public void LineTo(double x, double y)
        {
            Console.Out.WriteLine("Line: {0}, {1}", x, y);
            new Line((int)_x, (int)_y, (int)x, (int)y).FillFlags(_flags);
            _x = x;
            _y = y;
        }

        public void Curve3(double p2x, double p2y, double x, double y)
        {
            Console.Out.WriteLine("Bezier: {0}, {1}, {2}, {3}", p2x, p2y, x, y);
            new Bezier((int)_x, (int)_y, (int)p2x, (int)p2y, (int)x, (int)y).FillFlags(_flags);
            _x = x;
            _y = y;
        }

        public void Curve4(double p2x, double p2y, double p3x, double p3y, double x, double y)
        {
            // TODO: subdivide...
            _x = x;
            _y = y;
        }

        public void MoveTo(double x, double y)
        {
            Console.Out.WriteLine("Move: {0}, {1}", x, y);
            _x = x;
            _y = y;

            if (first)
            {
                _firstX = x;
                _firstY = y;
                first = false;
            }
        }

        public void CloseFigure()
        {
            LineTo(_firstX, _firstY);
        }

        #endregion
    }
}
