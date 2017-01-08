using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NRasterizer.Tests
{
    public class RendererTests
    {
        [Theory]
        [InlineData("Hello World", 64, 704, 64)] //pixel size == font size in these tests
        [InlineData("Hello", 10, 50, 10)] //pixel size == font size in these tests
        [InlineData("World", 10, 50, 10)] //pixel size == font size in these tests

        // \n is treated as carage return followed by newline in both unix and windows
        // technically in windows \n should retian the current x position and just move down by line height.
        // \r will just return to start of current line
        [InlineData("Hi\nWorld", 10, 50, 20)] // moves to newline(which will also reset the line start)
        [InlineData("Hello\nMe", 10, 50, 20)]  // moves to newline(which will also reset the line start)
        [InlineData("Hi\r\nWorld", 10, 50, 20)]  // reset the single line the moves to newline(which will also reset the line start)
        [InlineData("Hello\r\nMe", 10, 50, 20)] // reset the single line the moves to new line
        [InlineData("Hello\rMe", 10, 50, 10)] // sticks to single line but reset the start of line
        public void MesureText(string text, int fontSize, int expectedWidth, int expectedHeight)
        {
            // the default test factory will end up faking out spacing for a monospaced font.
            var renderer = TestFactory.CreateRenderer(text);

            var options = new TextOptions()
            {
                FontSize = fontSize,
                LineHeight = 1,
            };

            var size = renderer.Measure(text, options);

            Assert.Equal(expectedWidth, size.Width);
            Assert.Equal(expectedHeight, size.Height);
        }

        /// <summary>
        /// Last X position should be at (fontsize * letter count) + start position;
        /// Last Y position should be at fontsize  + start position;
        /// </summary>
        [Theory]
        [InlineData("a", 10, 10, 10, 20, 20)] 
        [InlineData("ab", 1, 10, 10, 12, 11)] 
        [InlineData("a\nb", 5, 10, 10, 15, 20)]
        [InlineData("a\tb", 1, 10, 10, 13, 11)] // this will break once tab support is added.
        public void RenderText(string text, int fontsize, int startX, int startY, int lastX, int lastY)
        {
            var rasterizer = new FakeGlyphRasterizer();

            // the default test factory will end up faking out spacing for a monospaced font.
            // the font will draw a line from top left to bottom right for each character
            var renderer = TestFactory.CreateRenderer(text, rasterizer);

            var options = new TextOptions()
            {
                FontSize = fontsize,
                LineHeight = 1
            };

            renderer.Render(startX, startY, text, options);

            var start = rasterizer.Points.First();
            var end = rasterizer.Points.Last();

            Assert.Equal(startX, start.X);
            Assert.Equal(startY, start.Y);
            
            Assert.Equal(lastX, end.X);
            Assert.Equal(lastY, end.Y);            
        }
    }
}
