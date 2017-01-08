using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NRasterizer.Tables
{
    /// <summary>
    /// alias binary reader methods to aline with opentype spec names to make translation easier.
    /// </summary>
    internal static class BinaryReaderExtensions
    {
        public static ushort USHORT(this BinaryReader reader) => reader.ReadUInt16();
        public static short SHORT(this BinaryReader reader) => reader.ReadInt16();
        public static uint ULONG(this BinaryReader reader) => reader.ReadUInt32();
        public static int LONG(this BinaryReader reader) => reader.ReadInt32();
        public static sbyte CHAR(this BinaryReader reader) => reader.ReadSByte();
        public static sbyte[] CHAR(this BinaryReader reader, int count)
        {
            var result = new sbyte[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = reader.ReadSByte();
            }
            return result;
        }

        private static byte[] BYTE(this BinaryReader input, int length)
        {
            return input.ReadBytes(length);
        }
    }
}
