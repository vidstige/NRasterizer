using System;
using System.Collections.Generic;

namespace NRasterizer
{
    public class CompositeGlyph
    {
        public class Composite
        {
            private readonly ushort _glyphIndex;
            private readonly Transformation _transformation;

            public Composite(ushort glyphIndex, Transformation transformation)
            {
                _glyphIndex = glyphIndex;
                _transformation = transformation;
            }

            public ushort GlyphIndex { get { return _glyphIndex; } }
            public Transformation Transformation { get { return _transformation; } }
        }

        private readonly List<Composite> _composites;

        public CompositeGlyph(List<Composite> composites)
        {
            _composites = composites;
        }

        private Glyph Transform(Transformation transformation, Glyph glyph)
        {
            var xs = (short[])glyph.X.Clone();
            var ys = (short[])glyph.Y.Clone();
            var n = xs.Length;
            for (int i = 0; i < n; i++)
            {
                var p = new Point<short>(xs[i], ys[i]);
                var pt = transformation.Transform(p);
                xs[i] = pt.X;
                ys[i] = pt.Y;
            }
            return new Glyph(xs, ys, glyph.On, glyph.EndPoints, glyph.Bounds); // TODO: Transform bounds too..
        }

        private T[] Concat<T>(T[] first, T[] second)
        {
            var result = new T[first.Length + second.Length];
            first.CopyTo(result, 0);
            second.CopyTo(result, first.Length);
            return result;
        }

        private Glyph Combine(Glyph first, Glyph second)
        {
            var xs = Concat(first.X, second.X);
            var ys = Concat(first.Y, second.Y);
            var ons = Concat(first.On, second.On);

            var endPoints = (ushort[])second.EndPoints.Clone();
            var offset = first.X.Length;
            for (int i = 0; i < endPoints.Length; i++)
            {
                endPoints[i] = (ushort)(endPoints[i] + offset);
            }

            return new Glyph(xs, ys, ons, Concat(first.EndPoints, endPoints), Bounds.For(first.Bounds, second.Bounds));
        }

        public Glyph Flatten(List<Glyph> glyphs)
        {
            Glyph flat = Glyph.Empty;
            List<Glyph> parts = new List<Glyph>();
            foreach (var composite in _composites)
            {
                flat = Combine(flat, Transform(composite.Transformation, glyphs[composite.GlyphIndex]));
                parts.Add(glyphs[composite.GlyphIndex]);
            }
            return flat;
        }
    }
}
