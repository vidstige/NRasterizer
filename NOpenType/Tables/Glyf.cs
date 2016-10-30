using System;
using System.Collections.Generic;
using System.IO;

namespace NRasterizer.Tables
{

    internal class Glyf
    {
        [Flags]
        enum Flag : byte
        {
            ControlPoint = 0,
            OnCurve = 1,
            XByte = 2,
            YByte = 4,
            Repeat = 8,
            XSignOrSame = 16,
            YSignOrSame = 32
        }

        private static bool HasFlag(Flag haystack, Flag needle)
        {
            return (haystack & needle) != 0;
        }

        private static Flag[] ReadFlags(BinaryReader input, int flagCount)
        {
            var result = new Flag[flagCount];
            int c = 0;
            int repeatCount = 0;
            var flag = (Flag)0;
            while (c < flagCount)
            {
                if (repeatCount > 0)
                {
                    repeatCount--;
                }
                else
                {
                    flag = (Flag)input.ReadByte();
                    if (HasFlag(flag, Flag.Repeat))
                    {
                        repeatCount = input.ReadByte();
                    }
                }
                result[c++] = flag;
            }
            return result;
        }

        private static short[] ReadCoordinates(BinaryReader input, int pointCount, Flag[] flags, Flag isByte, Flag signOrSame)
        {
            var xs = new short[pointCount];
            int x = 0;
            for (int i = 0; i < pointCount; i++)
            {
                int dx;
                if (HasFlag(flags[i], isByte))
                {
                    var b = input.ReadByte();
                    dx = HasFlag(flags[i], signOrSame) ? b : -b;
                }
                else
                {
                    if (HasFlag(signOrSame, flags[i]))
                    {
                        dx = 0;
                    }
                    else
                    {
                        dx = input.ReadInt16();
                    }
                }
                x += dx;
                xs[i] = (short)x; // TODO: overflow?
            }
            return xs;
        }

        private static Glyph ReadSimpleGlyph(BinaryReader input, int count, Bounds bounds)
        {
            var endPoints = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                endPoints[i] = input.ReadUInt16();
            }

            var instructionSize = input.ReadUInt16();
            var instructions = input.ReadBytes(instructionSize);

            // TODO: should this take the max points rather?
            var pointCount = endPoints[count - 1] + 1; // TODO: count can be zero?

            var flags = ReadFlags(input, pointCount);
            var xs = ReadCoordinates(input, pointCount, flags, Flag.XByte, Flag.XSignOrSame);
            var ys = ReadCoordinates(input, pointCount, flags, Flag.YByte, Flag.YSignOrSame);

            var onCurves = new bool[flags.Length];
            for (int i = flags.Length - 1; i >= 0; --i)
            {

                onCurves[i] = (flags[i] & Flag.OnCurve) == Flag.OnCurve;
            }

            return new Glyph(xs, ys, onCurves, endPoints, bounds);
        }

        private enum CompositeFlags: ushort
        {
            ArgsAreWords = 1,    // If this is set, the arguments are words; otherwise, they are bytes.
            ArgsAreXYValues = 2, // If this is set, the arguments are xy values; otherwise, they are points.
            RoundXYToGrid = 4,   // For the xy values if the preceding is true.
            WeHaveAScale = 8,    // This indicates that there is a simple scale for the component. Otherwise, scale = 1.0.
            Reserved = 16,       // This bit is reserved. Set it to 0.
            MoreComponents = 32, // Indicates at least one more glyph after this one.
            WeHaveXAndYScale = 64, // The x direction will use a different scale from the y direction.
            WeHaveATwoByTwo = 128, // There is a 2 by 2 transformation that will be used to scale the component.
            WeHaveInstructions = 256, // Following the last component are instructions for the composite character.
            UseMyMetrics = 512,  // If set, this forces the aw and lsb (and rsb) for the composite to be equal to those from this original glyph. This works for hinted and unhinted characters.
            OverlapCompound = 1024,  // If set, the components of the compound glyph overlap. Use of this flag is not required in OpenType — that is, it is valid to have components overlap without having this flag set. It may affect behaviors in some platforms, however. (See Apple’s specification for details regarding behavior in Apple platforms.)
            ScaledComponentOffset = 2048, // The composite is designed to have the component offset scaled.
            UnscaledComponentOffset = 4096 // The composite is designed not to have the component offset scaled.
        }

        private static bool HasFlag(CompositeFlags haystack, CompositeFlags needle)
        {
            return (haystack & needle) != 0;
        }

        private static CompositeGlyph ReadCompositeGlyph(BinaryReader input, int count, Bounds bounds, List<IGlyph> glyphs)
        {
            List<CompositeGlyph.Composite> result = new List<CompositeGlyph.Composite>();
            CompositeFlags flags;
            ushort glyphIndex;
            do {
                flags = (CompositeFlags)input.ReadUInt16();
                glyphIndex = input.ReadUInt16();

                short arg1;
                short arg2;
                short m00 = 1 << 14, m01 = 0, m10 = 0, m11 = 1 << 14;
                if (HasFlag(flags, CompositeFlags.ArgsAreWords)) {
                    arg1 = input.ReadInt16();
                    arg2 = input.ReadInt16();
                } else {
                    arg1 = input.ReadByte();
                    arg2 = input.ReadByte();
                    //USHORT arg1and2; /* (arg1 << 8) | arg2 */
                }

                short dx;
                short dy;
                if (HasFlag(flags, CompositeFlags.ArgsAreXYValues))
                {
                    dx = arg1;
                    dy = arg2;
                } else {
                    // args are points to be matched
                    // TODO: Implement
                    dx = 0;
                    dy = 0;
                }

                if (HasFlag(flags, CompositeFlags.WeHaveAScale)) {
                    short scale = input.ReadInt16(); // Format 2.14
                    m00 = scale;
                    m11 = scale;
                } else if (HasFlag(flags, CompositeFlags.WeHaveXAndYScale)) {
                    short xscale = input.ReadInt16(); // Format 2.14
                    short yscale = input.ReadInt16(); // Format 2.14
                    m00 = xscale;
                    m11 = yscale;
                } else if (HasFlag(flags, CompositeFlags.WeHaveATwoByTwo)) {
                    m00 = input.ReadInt16(); // Format 2.14
                    m01 = input.ReadInt16(); // Format 2.14
                    m10 = input.ReadInt16(); // Format 2.14
                    m11 = input.ReadInt16(); // Format 2.14
                }
                Transformation transformation = new Transformation(m00, m01, m10, m11, dx, dy);
                result.Add(new CompositeGlyph.Composite(glyphIndex, transformation));
            } while (HasFlag(flags, CompositeFlags.MoreComponents));

            if (HasFlag(flags, CompositeFlags.WeHaveInstructions))
            {
                //USHORT numInstr
                //BYTE instr[numInstr];
            }

            return new CompositeGlyph(result);
            //return Glyph.Empty;
        }

        internal static List<IGlyph> From(TableEntry table, GlyphLocations locations)
        {
            var glyphCount = locations.GlyphCount;

            var glyphs = new List<IGlyph>(glyphCount);
            for (int i = 0; i < glyphCount; i++)
            {
                var input = table.GetDataReader();
                input.BaseStream.Seek(locations.Offsets[i], SeekOrigin.Current);

                var length = locations.Offsets[i + 1] - locations.Offsets[i];
                if (length > 0)
                {
                    var contoursCount = input.ReadInt16();
                    var bounds = BoundsReader.ReadFrom(input);
                    if (contoursCount >= 0)
                    {
                        glyphs.Add(ReadSimpleGlyph(input, contoursCount, bounds));
                    }
                    else
                    {
                        glyphs.Add(ReadCompositeGlyph(input, -contoursCount, bounds, glyphs));
                    }
                }
                else
                {
                    glyphs.Add(Glyph.Empty);
                }
            }

            // Flatten all composites
            for (int i = 0; i < glyphs.Count; i++)
            {
                var composite = glyphs[i] as CompositeGlyph;
                if (composite != null)
                {
                    glyphs[i] = composite.Flatten(glyphs);
                }
            }
            return glyphs;
        }
    }
}
