using System;

namespace NRasterizer
{
    using Fixed2d14 = Int16;

    public class Transformation
    {
        private readonly Fixed2d14 _m00;
        private readonly Fixed2d14 _m01;
        private readonly Fixed2d14 _m10;
        private readonly Fixed2d14 _m11;
        private readonly short _dx;
        private readonly short _dy;

        public Transformation(Fixed2d14 m00, Fixed2d14 m01, Fixed2d14 m10, Fixed2d14 m11, short dx, short dy)
        {
            _m00 = m00;
            _m01 = m01;
            _m10 = m10;
            _m11 = m11;
            _dx = dx;
            _dy = dy;
        }

        public Point<short> Transform(Point<short> p)
        {
            var px = (long)p.X;
            var py = (long)p.Y;
            var x = (((long)_m00 * px + (long)_m01 * py) >> 14) + _dx;
            var y = (((long)_m10 * px + (long)_m11 * py) >> 14) + _dy;
            return new Point<short>((short)x, (short)y);
        }
    }
}
