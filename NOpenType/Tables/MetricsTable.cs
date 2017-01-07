using System;
using System.IO;

namespace NRasterizer.Tables
{
    /// <summary>
    /// this is the OS/2 windows metrics table
    /// </summary>
    /// <remarks>
    /// https://www.microsoft.com/typography/otspec/os2.htm
    /// </remarks>
    internal class MetricsTable
    {
        private readonly short sTypoAscender;
        private readonly short sTypoDescender;
        private readonly short sTypoLineGap;

        public MetricsTable(BinaryReader input)
        {
            // USHORT version    0x0005
            var version = input.USHORT();
            // SHORT xAvgCharWidth
            var xAvgCharWidth = input.SHORT();
            // USHORT usWeightClass
            var usWeightClass = input.USHORT();
            // USHORT usWidthClass
            var usWidthClass = input.USHORT();
            // USHORT fsType
            var fsType = input.USHORT();
            // SHORT ySubscriptXSize
            var ySubscriptXSize = input.SHORT();
            // SHORT ySubscriptYSize
            var ySubscriptYSize = input.SHORT();
            // SHORT ySubscriptXOffset
            var ySubscriptXOffset = input.SHORT();
            // SHORT ySubscriptYOffset
            var ySubscriptYOffset = input.SHORT();
            // SHORT ySuperscriptXSize
            var ySuperscriptXSize = input.SHORT();
            // SHORT ySuperscriptYSize
            var ySuperscriptYSize = input.SHORT();
            // SHORT ySuperscriptXOffset
            var ySuperscriptXOffset = input.SHORT();
            // SHORT ySuperscriptYOffset
            var ySuperscriptYOffset = input.SHORT();
            // SHORT yStrikeoutSize
            var yStrikeoutSize = input.SHORT();
            // SHORT yStrikeoutPosition
            var yStrikeoutPosition = input.SHORT();
            // SHORT sFamilyClass
            var sFamilyClass = input.SHORT();
            // BYTE panose[10]
            var panose = input.ReadBytes(10);
            // ULONG ulUnicodeRange1 Bits 0-31
            var ulUnicodeRange1 = input.ULONG();
            // ULONG ulUnicodeRange2 Bits 32-63
            var ulUnicodeRange2 = input.ULONG();
            // ULONG ulUnicodeRange3 Bits 64-95
            var ulUnicodeRange3 = input.ULONG();
            // ULONG ulUnicodeRange4 Bits 96-127
            var ulUnicodeRange4 = input.ULONG();
            // CHAR achVendID[4]
            var achVendID = input.CHAR(4);
            // USHORT fsSelection
            var fsSelection = input.USHORT();
            // USHORT usFirstCharIndex
            var usFirstCharIndex = input.USHORT();
            // USHORT usLastCharIndex
            var usLastCharIndex = input.USHORT();
            // SHORT sTypoAscender
            sTypoAscender = input.SHORT();
            // SHORT sTypoDescender
            sTypoDescender = input.SHORT();
            // SHORT sTypoLineGap
            sTypoLineGap = input.SHORT();
            // USHORT usWinAscent
            var usWinAscent = input.USHORT();
            // USHORT usWinDescent
            var usWinDescent = input.USHORT();
            // ULONG ulCodePageRange1    Bits 0-31
            var ulCodePageRange1 = input.ULONG();
            // ULONG ulCodePageRange2    Bits 32-63
            var ulCodePageRange2 = input.ULONG();
            // SHORT sxHeight
            var sxHeight = input.SHORT();
            // SHORT sCapHeight
            var sCapHeight = input.SHORT();
            // USHORT usDefaultChar
            var usDefaultChar = input.USHORT();
            // USHORT usBreakChar
            var usBreakChar = input.USHORT();
            // USHORT usMaxContext
            var usMaxContext = input.USHORT();
            // USHORT usLowerOpticalPointSize
            var usLowerOpticalPointSize = input.USHORT();
            // USHORT usUpperOpticalPointSize
            var usUpperOpticalPointSize = input.USHORT();

        }


        public int LineSpacing
        {
            get
            {
                // https://www.microsoft.com/typography/otspec/recom.htm#base
                return sTypoAscender - sTypoDescender + sTypoLineGap;
            }
        }

        public static MetricsTable From(TableEntry table)
        {
            var input = table.GetDataReader();

            return new MetricsTable(input);
        }
    }
}
