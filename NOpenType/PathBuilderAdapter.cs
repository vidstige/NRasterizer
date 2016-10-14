using System;

namespace NRasterizer
{
    // Helperfunctions to make GlyphPathBuilder build
    public class Point<T>
    {
        public readonly T x;
        public readonly T y;

        public Point(T x, T y)
        {
            this.x = x;
            this.y = y;
        }
        public Point(): this(default(T), default(T))  { }

        public T X { get { return x; } }
        public T Y { get { return y; } }
    }


    public class FtPoint: Point<float>
    {
        //public FtPoint(float x, float y): base(x, y) {}
    }

    public class FtPointD: Point<double>
    {
        public FtPointD() {}
        public FtPointD(FtPoint p): base(p.X, p.Y) {}
        public FtPointD(double x, double y): base(x, y) {}
    }

}

