using System;
using System.Collections.Generic;
using NRasterizer.Rasterizer;
namespace NRasterizer
{
    public class Glyph: IGlyph
    {
        private readonly short[] _x;
        private readonly short[] _y;
        private readonly bool[] _on;
        private readonly ushort[] _contourEndPoints;
        private readonly Bounds _bounds;

        public static readonly Glyph Empty = new Glyph(new short[0], new short[0], new bool[0], new ushort[0], Bounds.Zero);

        public Glyph(short[] x, short[] y, bool[] on, ushort[] contourEndPoints, Bounds bounds)
        {
            _x = x;
            _y = y;
            _on = on;
            _contourEndPoints = contourEndPoints;
            _bounds = bounds;
        }

        public Bounds Bounds { get { return _bounds; } }

        public short[] X { get { return _x; } }
        public short[] Y { get { return _y; } }
        public bool[] On { get { return _on; } }
        public ushort[] EndPoints { get { return _contourEndPoints; } }

    }
}
