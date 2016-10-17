using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using NRasterizer.Rasterizer;
using Moq;

namespace NRasterizer.Tests
{
    [TestFixture]
    public class Given_Glyph
    {
        private Renderer _renderer;
        private Mock<IGlyphRasterizer> _mock;
        private MockSequence _sequence;

        [SetUp]
        public void SetUp()
        {
            _mock = new Mock<IGlyphRasterizer>(MockBehavior.Strict);
            _sequence = new MockSequence();
            _mock.InSequence(_sequence).Setup(r => r.BeginRead(1));
            _renderer = new Renderer(null, _mock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _mock.VerifyAll();
        }

        private void Render(Glyph glyph)
        {
            _renderer.RenderGlyph(0, 0, glyph);
        }

        private void StartAt(int x, int y)
        {   
            _mock.InSequence(_sequence).Setup(d => d.MoveTo(x, y));
        }

        private void AssertLineTo(int x, int y)
        {
            _mock.InSequence(_sequence).Setup(d => d.LineTo(x, y));
        }

        private void AssertBezierTo(int cx, int cy, int endx, int endy)
        {
            _mock.InSequence(_sequence).Setup(d => d.Curve3(cx, cy, endx, endy));
        }

        private void AssertContourDone()
        {
            //_mock.InSequence(_sequence).Setup(d => d.CloseFigure()).Callback(() => System.Console.WriteLine("called"));
            _mock.Setup(d => d.CloseFigure());
            _mock.Setup(d => d.EndRead());
        }

        [Test]
        public void With_Four_Line_Countour()
        {
            var x = new short[] { 0, 128, 128, 0 };
            var y = new short[] { 0, 0, 128, 128 };
            var on = new bool[] { true, true, true, true };

            StartAt(0, 0);
            AssertLineTo(2, 0);
            AssertLineTo(2, 2);
            AssertLineTo(0, 2);
            AssertLineTo(0, 0);
            AssertContourDone();

            Render(new Glyph(x, y, on, new ushort[] { 3 }, null));
        }

        [Test]
        public void With_Line_And_Bezier_Countour()
        {
            var x = new short[] { 0, 128, 128, 0 };
            var y = new short[] { 0, 0, 128, 128 };
            var on = new bool[] { true, true, false, true };

            StartAt(0, 0);
            AssertLineTo(2, 0);
            AssertBezierTo(2, 2, 0, 2);
            AssertLineTo(0, 0);
            AssertContourDone();

            Render(new Glyph(x, y, on, new ushort[] { 3 }, null));
        }
    }
}
