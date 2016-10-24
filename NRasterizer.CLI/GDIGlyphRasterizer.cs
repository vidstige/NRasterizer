using System;
using System.Drawing;

namespace NRasterizer.CLI
{
    public class GDIGlyphRasterizer: IGlyphRasterizer
    {
        private System.Drawing.Drawing2D.GraphicsPath ps;
        private float lastX;
        private float lastY;

        private Graphics _graphics;
        private Brush _brush;

        public GDIGlyphRasterizer(Graphics g, Brush brush)
        {
            _graphics = g;
            _brush = brush;
            ps = new System.Drawing.Drawing2D.GraphicsPath();
        }

        #region IGlyphRasterizer implementation

        public void BeginRead(int countourCount)
        {
        }

        public void EndRead()
        {
        }

        public void CloseFigure()
        {
            ps.CloseFigure();
        }

        public void Curve3(double p2x, double p2y, double x, double y)
        {
            //from http://stackoverflow.com/questions/9485788/convert-quadratic-curve-to-cubic-curve
            //Control1X = StartX + (.66 * (ControlX - StartX))
            //Control2X = EndX + (.66 * (ControlX - EndX))


            float c1x = lastX + (float)((2f / 3f) * (p2x - lastX));
            float c1y = lastY + (float)((2f / 3f) * (p2y - lastY));
            //---------------------------------------------------------------------
            float c2x = (float)(x + ((2f / 3f) * (p2x - x)));
            float c2y = (float)(y + ((2f / 3f) * (p2y - y)));
            //---------------------------------------------------------------------
            ps.AddBezier(
                new PointF(lastX, lastY),
                new PointF(c1x, c1y),
                new PointF(c2x, c2y),
                new PointF(lastX = (float)x, lastY = (float)y));

        }

        public void Curve4(double p2x, double p2y, double p3x, double p3y, double x, double y)
        {
            // ps.Curve4(p2x, p2y, p3x, p3y, x, y);

            ps.AddBezier(
                new PointF(lastX, lastY),
                new PointF((float)p2x, (float)p2y),
                new PointF((float)p3x, (float)p3y),
                new PointF(lastX = (float)x, lastY = (float)y));

        }

        public void LineTo(double x, double y)
        {
            ps.AddLine(
                new PointF(lastX, lastY),
                new PointF(lastX = (float)x, lastY = (float)y));
        }

        public void MoveTo(double x, double y)
        {
            lastX = (float)x;
            lastY = (float)y;
        }

        public void Flush()
        {
            _graphics.FillPath(_brush, ps);
        }

        #endregion
    }
}

