//Apache2, 2014-2016,   WinterDev
using System;

namespace NRasterizer
{
    /// <summary>
    /// Configuration options for controlling how text is rendered.
    /// </summary>
    public class TextOptions
    {
        // currently this only include fontsize, but it is also that place where you can/will configure things like 
        // kerning, font weight, font style, strikethrough, line height etc.

        /// <summary>
        /// Gets or sets the size of the font in point.
        /// </summary>
        /// <value>
        /// The size of the font in point.
        /// </value>
        /// <remarks> 1 point is 1/72 of an inch based on the Resolution of the <see cref="IGlyphRasterizer"/>.</remarks>
        public int FontSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the line height based on multiple of the fonts actual height.
        /// </summary>
        /// <value>
        /// The height of the line.
        /// </value>
        public float LineHeight { get; set; } = 1;
    }
}