using System;

namespace NRasterizer
{
    public interface IGlyphRasterizer
    {
        void BeginRead(int countourCount);
        void EndRead();

        void LineTo(double x, double y);
        void Curve3(double p2x, double p2y, double x, double y);
        void Curve4(double p2x, double p2y, double p3x, double p3y, double x, double y);
        void MoveTo(double x, double y);
        void CloseFigure();
    }
}

