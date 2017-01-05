using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NRasterizer.Tables;

namespace NRasterizer.Tests
{
    public class TestFactory
    {
        internal static CharacterMap CreateCharacterMap(char[] characters)
        {
            var min = characters.Min();
            var max = characters.Max();
            var segCount = characters.Length;
            var startCodes = new List<ushort>();
            var endCodes = new List<ushort>();
            var idDeltas = new List<ushort>();
            var idRangeOffset = new List<ushort>();
            var glyphIdArray = new List<ushort>();
            int idx = -1;
            foreach (var c in characters)
            {
                idx++;
                startCodes.Add(c);
                endCodes.Add(c);
                idDeltas.Add((ushort)(65536 - c + idx));
                idRangeOffset.Add(0);
                glyphIdArray.Add(c);
            };

            return new CharacterMap(segCount, startCodes.ToArray(), endCodes.ToArray(), idDeltas.ToArray(), idRangeOffset.ToArray(), glyphIdArray.ToArray());
        }

        // to mean that lettersize becomes pixel size the resolution must be 72 if letterSize == EmSquare.Size
        public static Renderer CreateRenderer(string supportedCharacters, int lettersize = EmSquare.Size, int resolution = 72)
        {
            //lets fake out a typeface with all stnadrd cahractes
            var mockRasterizer = new Mock<IGlyphRasterizer>();
                    
            mockRasterizer.Setup(x => x.Resolution).Returns(72); 

            var fakeTypeface = CreateTypeface(supportedCharacters, lettersize);
            var renderer = new Renderer(fakeTypeface, mockRasterizer.Object);

            return renderer;
        }

        public static Typeface CreateTypeface(string supportedCharacters, int lettersize = EmSquare.Size)
        {
            var characters = supportedCharacters
                .Replace("\n", "") //should these be supported or not? will it matter???
                .Replace("\r", "")
                .Replace("\t", "")
                .OrderBy(x => x)
                .Distinct()
                .ToArray();

            List<Glyph> glyphs = new List<Glyph>();
            List<ushort> letterWidths = new List<ushort>();
            List<short> leftSideBearings = new List<short>();
            foreach (var c in characters)
            {
                var g = new Glyph(new short[0], new short[0], new bool[0], new ushort[0], new Bounds(0,0,0,0));
                glyphs.Add(g);
                letterWidths.Add((ushort)lettersize);
                leftSideBearings.Add(0);
            }

            var cmap = CreateCharacterMap(characters);
            var h_metrics = new HorizontalMetrics(letterWidths.ToArray(), leftSideBearings.ToArray());
            return new Typeface(new Bounds(0, 0, (short)lettersize, (short)lettersize), 1, glyphs, new[] { cmap }.ToList(), h_metrics);
        }
    }
}
