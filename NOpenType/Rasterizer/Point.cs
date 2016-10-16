using System;

namespace NRasterizer
{
    public class Point
    {
        private readonly short _x;
        private readonly short _y;
        private readonly bool _on;

        public Point(short x, short y, bool on)
        {
            _x = x;
            _y = y;
            _on = on;
        }

        public short X { get { return _x; } }
        public short Y { get { return _y; } }
        public bool On { get { return _on; } }
    }
}
