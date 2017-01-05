using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRasterizer.Tests
{
    [TestFixture]
    public class RendererTests
    {
        [Test]
        [TestCase("Hello World", 64, 704, 64)] //pixel size == font size in these tests
        [TestCase("Hello", 10, 50, 10)] //pixel size == font size in these tests
        [TestCase("World", 10, 50, 10)] //pixel size == font size in these tests
        public void MesureText(string text, int fontSize, int expectedWidth, int expectedHeight)
        {
            // the default test factory will end up faking out spacing for a monospaced font.
            var renderer = TestFactory.CreateRenderer(text);

            var options = new TextOptions()
            {
                FontSize = fontSize
            };

            var size = renderer.Measure(text, options);

            Assert.AreEqual(expectedWidth, size.Width);
            Assert.AreEqual(expectedHeight, size.Height);
        }
    }
}
