using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NRasterizer.Tests
{
    public class CharacterMapTests
    {
        [Fact]
        public void CharacterToGlyphIndex_NoRangeOffset()
        {
            ushort charAsInt = 'a';
            var segCount = 1;
            var startCodes = new ushort[] {
                charAsInt
            };
            var endCodes = new ushort[] {
                charAsInt
            };
            var idDeltas = new ushort[] {
                (ushort) (65536 - charAsInt)
            };
            var idRangeOffset = new ushort[] {
                0
            };
            var glyphIdArray = new ushort[] {
            };

            var map = new CharacterMap(segCount, startCodes, endCodes, idDeltas, idRangeOffset, glyphIdArray);

            var index = map.CharacterToGlyphIndex(charAsInt);

            Assert.Equal(0, index);
        }
        
        [Fact]
        public void CharacterToGlyphIndex_WithRangeOffset()
        {
            ushort charAsInt = 'a';
            var segCount = 1;
            var startCodes = new ushort[] {
                charAsInt
            };
            var endCodes = new ushort[] {
                charAsInt
            };
            var idDeltas = new ushort[] {
                (ushort) (65536 - charAsInt)
            };
            var idRangeOffset = new ushort[] {
                2
            };
            var glyphIdArray = new ushort[] {
                54
            };

            var map = new CharacterMap(segCount, startCodes, endCodes, idDeltas, idRangeOffset, glyphIdArray);

            var index = map.CharacterToGlyphIndex(charAsInt);

            Assert.Equal(54, index);
        }

        [Fact]
        public void CharacterToGlyphIndex_FakeFactorMap()
        {
            var chars = new char[]
                        {
                            'a',
                            'b',
                            'c',
                            'd',
                            '9',
                            '@'
                        };

            var map = TestFactory.CreateCharacterMap(chars);

            for(var i = 0; i< chars.Length; i++)
            {
                var idx = map.CharacterToGlyphIndex(chars[i]);
                Assert.Equal(i, idx);
            }
        }

    }
}