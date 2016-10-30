using System;
using System.Collections.Generic;

namespace NRasterizer
{
    public class CompositeGlyph: IGlyph
    {
        public class Composite
        {
            private readonly ushort _glyphIndex;
            private readonly Transform _transform;
            public Composite(ushort glyphIndex, Transform transform)
            {
                _glyphIndex = glyphIndex;
                _transform = transform;
            }

            public ushort GlyphIndex { get { return _glyphIndex; } }
            public Transform Transform { get { return _transform; } }
        }

        private readonly List<Composite> _composites;

        public CompositeGlyph(List<Composite> composites)
        {
            _composites = composites;
        }

        private IGlyph Transform(Transform transform, IGlyph glyph)
        {
            return glyph;
        }

        private T[] Concat<T>(T[] first, T[] second)
        {
            var result = new T[first.Length + second.Length];
            first.CopyTo(result, 0);
            second.CopyTo(result, first.Length);
            return result;
        }

        private IGlyph Combine(IGlyph first, IGlyph second)
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

            return new Glyph(xs, ys, ons, Concat(first.EndPoints, endPoints), null);
        }

        public IGlyph Flatten(List<IGlyph> glyphs)
        {
            IGlyph flat = Glyph.Empty;
            List<IGlyph> parts = new List<IGlyph>();
            foreach (var composite in _composites)
            {
                flat = Combine(flat, Transform(composite.Transform, glyphs[composite.GlyphIndex]));
                parts.Add(glyphs[composite.GlyphIndex]);
            }

            return flat;
        }

        #region IGlyph implementation

        public Bounds Bounds { get { throw new NotSupportedException(); } }

        public short[] X { get { throw new NotSupportedException(); } }

        public short[] Y { get { throw new NotSupportedException(); } }

        public bool[] On { get { throw new NotSupportedException(); } }

        public ushort[] EndPoints { get { throw new NotSupportedException(); } }

        #endregion

    }
}
