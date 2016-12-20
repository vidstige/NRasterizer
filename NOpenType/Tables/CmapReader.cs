using System;
using System.Collections.Generic;
using System.IO;

namespace NRasterizer.Tables
{
    internal class CmapReader
    {
        private static UInt16[] ReadUInt16Array(BinaryReader input, int length)
        {
            var result = new UInt16[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = input.ReadUInt16();
            }
            return result;
        }

        private static CharacterMap ReadCharacterMap(CMapEntry entry, BinaryReader input)
        {
            // I want to thank Microsoft for not giving a simple count on the glyphIdArray
            long tableStart = input.BaseStream.Position;

            var format = input.ReadUInt16();
            var length = input.ReadUInt16();

            long tableEndAt = tableStart + length;


            if (format == 4)
            {
                var version = input.ReadUInt16();
                var segCountX2 = input.ReadUInt16();
                var searchRange = input.ReadUInt16();
                var entrySelector = input.ReadUInt16();
                var rangeShift = input.ReadUInt16();

                var segCount = segCountX2 / 2;

                var endCode = ReadUInt16Array(input, segCount); // last = 0xffff. What does that mean??

                input.ReadUInt16(); // Reserved = 0               

                var startCode = ReadUInt16Array(input, segCount);
                var idDelta = ReadUInt16Array(input, segCount);
                var idRangeOffset = ReadUInt16Array(input, segCount);

                // I want to thank Microsoft for not giving a simple count on the glyphIdArray
                //var glyphIdArrayLength = (int)((input.BaseStream.Position - tableStart) / sizeof(UInt16));
                //int glyphIdArrayLength = FindGlyphIdArrayLenInBytes(idRangeOffset) / 2;
                int glyphIdArrayLength = (int)(tableEndAt - input.BaseStream.Position) / 2;
                var glyphIdArray = ReadUInt16Array(input, glyphIdArrayLength);

                return new CharacterMap(segCount, startCode, endCode, idDelta, idRangeOffset, glyphIdArray);
            }
            else if (format == 0)
            {

                ushort language = input.ReadUInt16();
                byte[] only256Glyphs = input.ReadBytes(256);
                ushort[] only256UInt16Glyphs = new ushort[256];
                for (int i = 255; i >= 0; --i)
                {
                    //expand
                    only256UInt16Glyphs[i] = only256Glyphs[i];
                }
                //convert to format4 cmap table
                return new CharacterMap(1, new ushort[] { 0 }, new ushort[] { 255 }, null, null, only256UInt16Glyphs);
            }
            throw new NRasterizerException("Unknown cmap subtable: " + format);
        }
        //static int FindGlyphIdArrayLenInBytes(ushort[] idRangeOffset)
        //{
        //    //1. find max OffsetValue (in bytes unit)
        //    //this is the possible value to reach from the idRangeOffsetRecord 
        //    ushort max = 0;
        //    int foundAt = 0;
        //    for (int i = idRangeOffset.Length - 1; i >= 0; --i)
        //    {
        //        ushort off = idRangeOffset[i];
        //        if (off > max)
        //        {
        //            max = off;
        //            foundAt = i;
        //        }
        //    }
        //    //----------------------------
        //    //2. then offset with current found record
        //    return max - (foundAt * 2); //*2 = to byte unit 
        //}
        private class CMapEntry
        {
            private readonly UInt16 _platformId;
            private readonly UInt16 _encodingId;
            private readonly UInt32 _offset;

            public CMapEntry(UInt16 platformId, UInt16 encodingId, UInt32 offset)
            {
                _platformId = platformId;
                _encodingId = encodingId;
                _offset = offset;
            }
            public UInt16 PlatformId { get { return _platformId; } }
            public UInt16 EncodingId { get { return _encodingId; } }
            public UInt32 Offset { get { return _offset; } }
        }

        internal static List<CharacterMap> From(TableEntry table)
        {
            var input = table.GetDataReader();

            var version = input.ReadUInt16(); // 0
            var tableCount = input.ReadUInt16();

            var entries = new List<CMapEntry>(tableCount);
            for (int i = 0; i < tableCount; i++)
            {
                var platformId = input.ReadUInt16();
                var encodingId = input.ReadUInt16();
                var offset = input.ReadUInt32();
                entries.Add(new CMapEntry(platformId, encodingId, offset));
            }

            var result = new List<CharacterMap>(tableCount);
            foreach (var entry in entries)
            {
                var subtable = table.GetDataReader();
                subtable.BaseStream.Seek(entry.Offset, SeekOrigin.Current);
                result.Add(ReadCharacterMap(entry, subtable));
            }

            return result;
        }
    }
}