using System;

namespace NRasterizer
{
    // Helper functions to make GlyphPathBuilder build
    struct Point<T>
    {
        public readonly T x;
        public readonly T y;

        public Point(T x, T y)
        {
            this.x = x;
            this.y = y;
        }


        public T X { get { return x; } }
        public T Y { get { return y; } }
    } 

}

