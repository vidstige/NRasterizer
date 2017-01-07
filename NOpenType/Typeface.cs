using NRasterizer.Tables;
using System.Collections.Generic;

namespace NRasterizer
{
    public class Typeface
    {
        private readonly Bounds _bounds;
        private readonly ushort _unitsPerEm;
        private readonly List<Glyph> _glyphs;
        private readonly List<CharacterMap> _cmaps;
        private readonly HorizontalMetrics _horizontalMetrics;
        private readonly int _lineSpacing;

        internal Typeface(Bounds bounds, ushort unitsPerEm, int lineSpacing, List<Glyph> glyphs, List<CharacterMap> cmaps, HorizontalMetrics horizontalMetrics)
        {
            _bounds = bounds;
            _unitsPerEm = unitsPerEm;
            _lineSpacing = lineSpacing;
            _glyphs = glyphs;
            _cmaps = cmaps;
            _horizontalMetrics = horizontalMetrics;
        }

        public int LookupIndex(char character)
        {
            // TODO: What if there are none or several tables?
            return _cmaps[0].CharacterToGlyphIndex(character);
        }

        public Glyph Lookup(char character)
        {
            return _glyphs[LookupIndex(character)];
        }

        public ushort GetAdvanceWidth(char character)
        {
            return _horizontalMetrics.GetAdvanceWidth(LookupIndex(character));
        }

        public int LineSpacing {get { return _lineSpacing; } }
        public Bounds Bounds { get { return _bounds; } }
        public ushort UnitsPerEm { get { return _unitsPerEm; } }
        public List<Glyph> Glyphs { get { return _glyphs; } }
    }
}
