using System;
using System.Collections.Generic;
using System.IO;

namespace NRasterizer.Tables
{
    internal class HorizontalMetrics
    {
        private readonly ushort[] _advanceWidths;
        private readonly short[] _leftSideBearings;

        internal HorizontalMetrics(ushort[] advanceWidths, short[] leftSideBearings)
        {
            _advanceWidths = advanceWidths;
            _leftSideBearings = leftSideBearings;
        }

        public ushort GetAdvanceWidth(int index)
        {
            return _advanceWidths[index];
        }

        public static HorizontalMetrics From(TableEntry table, UInt16 count, UInt16 numGlyphs)
        {
            var input = table.GetDataReader();
            var advanceWidths = new List<ushort>(numGlyphs);
            var leftSideBearings = new List<short>(numGlyphs);

            for (int i = 0; i < count; i++)
            {
                advanceWidths.Add(input.ReadUInt16());
                leftSideBearings.Add(input.ReadInt16());
            }

            var advanceWidth = advanceWidths[advanceWidths.Count - 1];
            for (int i = 0; i < numGlyphs - count; i++)
            {
                advanceWidths.Add(advanceWidth);
                leftSideBearings.Add(input.ReadInt16());
            }

            return new HorizontalMetrics(advanceWidths.ToArray(), leftSideBearings.ToArray());
        }
    }
}
