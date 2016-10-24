using System;

namespace NRasterizer
{
    public class ToPixelRasterizer: IGlyphRasterizer
    {
        private readonly IGlyphRasterizer _inner;
        private readonly float _x;
        private readonly float _y;
        private readonly int _m;
        private readonly int _d;

        public ToPixelRasterizer(float x, float y, int multiplyer, int divider, IGlyphRasterizer inner)
        {
            _x = x;
            _y = y;
            _m = multiplyer;
            _d = divider;
            _inner = inner;
        }

        private double X(double x)
        {
            return _m * (_x + x) / _d;
        }
        private double Y(double y)
        {
            return _m * (_y + EmSquare.Size - y) / _d;
        }

        #region IGlyphRasterizer implementation

        public void BeginRead(int countourCount)
        {
            _inner.BeginRead(countourCount);
        }

        public void EndRead()
        {
            _inner.EndRead();
        }

        public void LineTo(double x, double y)
        {
            _inner.LineTo(X(x), Y(y));
        }

        public void Curve3(double p2x, double p2y, double x, double y)
        {
            _inner.Curve3(X(p2x), Y(p2y), X(x), Y(y));
        }

        public void Curve4(double p2x, double p2y, double p3x, double p3y, double x, double y)
        {
            _inner.Curve4(
                X(p2x), Y(p2y),
                X(p3x), Y(p3y),
                X(x), Y(y));
            
        }
        public void MoveTo(double x, double y)
        {
            _inner.MoveTo(X(x), Y(y));
        }
        public void CloseFigure()
        {
            _inner.CloseFigure();
        }

        public void Flush()
        {
            _inner.Flush();
        }

        #endregion
    }
}

