using System;

namespace NRasterizer
{
    public class TranslatingRasterizer: IGlyphRasterizer
    {
        private readonly IGlyphRasterizer _inner;
        private readonly float _x;
        private readonly float _y;

        public TranslatingRasterizer(float x, float y, IGlyphRasterizer inner)
        {
            _x = x;
            _y = y;
            _inner = inner;
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
            _inner.LineTo(_x + x, _y + y);
        }

        public void Curve3(double p2x, double p2y, double x, double y)
        {
            _inner.Curve3(_x + p2x, _y + p2y, _x + x, _y + y);
        }

        public void Curve4(double p2x, double p2y, double p3x, double p3y, double x, double y)
        {
            _inner.Curve4(
                _x + p2x, _y + p2y,
                _x + p3x, _y + p3y,
                _x + x, _y + y);
            
        }
        public void MoveTo(double x, double y)
        {
            _inner.MoveTo(_x + x, _y + y);
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

