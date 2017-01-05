using System;

namespace NRasterizer
{
    internal static class Constants
    {
        public const short EmSquareSize = 2048;

        public const int PointsPerInch = 72;

        internal const int FontToPixelDivisor = EmSquareSize * PointsPerInch;
    }
}

