using System;

namespace NRasterizer
{
    public interface IGlyph
    {
        Bounds Bounds { get; }

        short[] X { get; }
        short[] Y { get; }
        bool[] On { get; }
        ushort[] EndPoints { get; }
    }
}
