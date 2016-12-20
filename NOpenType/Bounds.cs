using System;

namespace NRasterizer
{
    public class Bounds
    {
        private readonly short _xmin;
        private readonly short _ymin;
        private readonly short _xmax;
        private readonly short _ymax;

        public static readonly Bounds Zero = new Bounds(0, 0, 0, 0);

        public Bounds(short xmin, short ymin, short xmax, short ymax)
        {
            _xmin = xmin;
            _ymin = ymin;
            _xmax = xmax;
            _ymax = ymax;
        }

        public short XMin { get { return _xmin; } }
        public short YMin { get { return _ymin; } }
        public short XMax { get { return _xmax; } }
        public short YMax { get { return _ymax; } }

        /// <summary>
        /// Computes the bounding box for two other bounding boxes
        /// </summary>
        /// <param name="bounds">First bounds</param>
        /// <param name="bounds2">Second bounds</param>
        public static Bounds For(Bounds first, Bounds second)
        {
            return new Bounds(
                Math.Min(first._xmin, second._xmin),
                Math.Min(first._ymin, second._ymin),
                Math.Max(first._xmax, second._xmax),
                Math.Max(first._ymax, second._ymax));
        }
    }
}
