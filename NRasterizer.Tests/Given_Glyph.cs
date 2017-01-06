using System.Linq;
using System.Collections.Generic;
using NRasterizer.Rasterizer;
using Xunit;
using Moq;

namespace NRasterizer.Tests
{
    public class Given_Glyph
    {
        private Renderer _renderer;
        private Mock<IGlyphRasterizer> _mock;
        private MockSequence _sequence;
        private int Scaler = 64;

        public void SetUp()
        {
            _mock = new Mock<IGlyphRasterizer>(MockBehavior.Strict);
            _sequence = new MockSequence();
            _mock.InSequence(_sequence).Setup(r => r.BeginRead(1));
            _renderer = new Renderer(null, _mock.Object);
        }

        public void Teardown()
        {
            _mock.VerifyAll();
        }

        private void Render(Glyph glyph)
        {
            var renderScaler = Renderer.FontToPixelDivisor / Scaler;

            _renderer.RenderGlyph(new Renderer.GlyphLayout {
                glyph = glyph
            }, renderScaler);
        }

        private void StartAt(double x, double y)
        {   
            _mock.InSequence(_sequence).Setup(d => d.MoveTo(x, y));
        }

        private void AssertLineTo(double x, double y)
        {
            _mock.InSequence(_sequence).Setup(d => d.LineTo(x, y));
        }

        private void AssertBezierTo(double cx, double cy, double endx, double endy)
        {
            _mock.InSequence(_sequence).Setup(d => d.Curve3(cx, cy, endx, endy));
        }

        private void AssertContourDone()
        {
            //_mock.InSequence(_sequence).Setup(d => d.CloseFigure()).Callback(() => System.Console.WriteLine("called"));
            _mock.Setup(d => d.CloseFigure());
            _mock.Setup(d => d.EndRead());
        }

        [Fact]
        public void With_Four_Line_Countour()
        {
            SetUp();
            var x = new short[] { 0, 128, 128, 0 };
            var y = new short[] { 0, 0, 128, 128 };
            var on = new bool[] { true, true, true, true };

            // These coordinates sytems are flipped in y-direction by EM-square baseline (2048) and then scaled by 1/64
            StartAt(0, 32);
            AssertLineTo(2, 32);
            AssertLineTo(2, 30);
            AssertLineTo(0, 30);
            AssertLineTo(0, 32);
            AssertContourDone();

            Render(new Glyph(x, y, on, new ushort[] { 3 }, null));

            Teardown();
        }

        [Fact]
        public void With_Line_And_Bezier_Countour()
        {
            SetUp();

            var x = new short[] { 0, 128, 128, 0 };
            var y = new short[] { 0, 0, 128, 128 };
            var on = new bool[] { true, true, false, true };

            // These coordinates sytems are flipped in y-direction by EM-square baseline (2048) and then scaled by 1/64
            StartAt(0, EmSquare.Size / Scaler);
            AssertLineTo(128 / Scaler, EmSquare.Size / Scaler);
            AssertBezierTo(128 / Scaler, (EmSquare.Size - 128) / Scaler, 0, (EmSquare.Size - 128) / Scaler);
            AssertLineTo(0, EmSquare.Size / Scaler);
            AssertContourDone();

            Render(new Glyph(x, y, on, new ushort[] { 3 }, null));

            Teardown();
        }
    }
}
