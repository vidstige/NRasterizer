//Apache2, 2014-2016,   WinterDev
using System;
using System.Collections.Generic;
using System.Linq;

namespace NRasterizer
{
    public class Renderer
    {
        private const int PointsPerInch = 72;

        internal const int FontToPixelDivisor = EmSquare.Size * PointsPerInch;

        private readonly IGlyphRasterizer _rasterizer;
        private readonly Typeface _typeface;

        public Renderer(Typeface typeface, IGlyphRasterizer rasterizer)
        {
            _typeface = typeface;
            _rasterizer = rasterizer;
        }

        /// <summary>
        /// Renders the glyph to the configured rasterizer.
        /// </summary>
        /// <param name="glyphLayout">The glyph layout.</param>
        /// <param name="scalingFactor">The scaling factor.</param>
        internal void RenderGlyph(GlyphLayout glyphLayout, int scalingFactor)
        {
            int x = glyphLayout.TopLeft.X;
            int y = glyphLayout.TopLeft.Y;
            Glyph glyph = glyphLayout.glyph;

            var rasterizer = new ToPixelRasterizer(x, y, scalingFactor, FontToPixelDivisor, _rasterizer);

            ushort[] contours = glyph.EndPoints;
            short[] xs = glyph.X;
            short[] ys = glyph.Y;
            bool[] onCurves = glyph.On;

            int npoints = xs.Length;
            int startContour = 0;
            int cpoint_index = 0;

            rasterizer.BeginRead(contours.Length);

            int lastMoveX = 0;
            int lastMoveY = 0;

            int controlPointCount = 0;
            for (int i = 0; i < contours.Length; i++)
            {
                int nextContour = contours[startContour] + 1;
                bool isFirstPoint = true;
                Point<int> secondControlPoint = new Point<int>();
                Point<int> thirdControlPoint = new Point<int>();
                bool justFromCurveMode = false;

                for (; cpoint_index < nextContour; ++cpoint_index)
                {

                    short vpoint_x = xs[cpoint_index];
                    short vpoint_y = ys[cpoint_index];
                    if (onCurves[cpoint_index])
                    {
                        //on curve
                        if (justFromCurveMode)
                        {
                            switch (controlPointCount)
                            {
                                case 1:
                                    {
                                        rasterizer.Curve3(
                                            secondControlPoint.x,
                                            secondControlPoint.y,
                                            vpoint_x,
                                            vpoint_y);
                                    }
                                    break;
                                case 2:
                                    {
                                        rasterizer.Curve4(
                                                secondControlPoint.x, secondControlPoint.y,
                                                thirdControlPoint.x, thirdControlPoint.y,
                                                vpoint_x, vpoint_y);
                                    }
                                    break;
                                default:
                                    {
                                        throw new NotSupportedException();
                                    }
                            }
                            controlPointCount = 0;
                            justFromCurveMode = false;
                        }
                        else
                        {
                            if (isFirstPoint)
                            {
                                isFirstPoint = false;
                                lastMoveX = vpoint_x;
                                lastMoveY = vpoint_y;
                                rasterizer.MoveTo(lastMoveX, lastMoveY);
                            }
                            else
                            {
                                rasterizer.LineTo(vpoint_x, vpoint_y);
                            }
                        }
                    }
                    else
                    {
                        switch (controlPointCount)
                        {
                            case 0:
                                {
                                    secondControlPoint = new Point<int>(vpoint_x, vpoint_y);
                                }
                                break;
                            case 1:
                                {
                                    //we already have prev second control point
                                    //so auto calculate line to 
                                    //between 2 point
                                    Point<int> mid = GetMidPoint(secondControlPoint, vpoint_x, vpoint_y);
                                    //----------
                                    //generate curve3
                                    rasterizer.Curve3(
                                        secondControlPoint.x, secondControlPoint.y,
                                        mid.x, mid.y);
                                    //------------------------
                                    controlPointCount--;
                                    //------------------------
                                    //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                    secondControlPoint = new Point<int>(vpoint_x, vpoint_y);
                                }
                                break;
                            default:
                                {
                                    throw new NotSupportedException("Too many control points");
                                }
                        }

                        controlPointCount++;
                        justFromCurveMode = true;
                    }
                }
                //--------
                //close figure
                //if in curve mode
                if (justFromCurveMode)
                {
                    switch (controlPointCount)
                    {
                        case 0: break;
                        case 1:
                            {
                                rasterizer.Curve3(
                                    secondControlPoint.x, secondControlPoint.y,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        case 2:
                            {
                                rasterizer.Curve4(
                                    secondControlPoint.x, secondControlPoint.y,
                                    thirdControlPoint.x, thirdControlPoint.y,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        default:
                            { throw new NotSupportedException("Too many control points"); }
                    }
                    justFromCurveMode = false;
                    controlPointCount = 0;
                }
                rasterizer.CloseFigure();
                //--------                   
                startContour++;
            }
            rasterizer.EndRead();
        }

        private static Point<int> GetMidPoint(Point<int> v1, int v2x, int v2y)
        {
            return new Point<int>((v1.X + v2x) / 2, (v1.Y + v2y) / 2);
        }

        private static Point<int> GetMidPoint(Point<int> v1, Point<int> v2)
        {
            return new Point<int>((v1.x + v2.x) / 2, (v1.y + v2.y) / 2);
        }

        /// <summary>
        /// Renders the specified text at the specified X and Y position.
        /// </summary>
        /// <param name="x">The x postion in pixels to draw the text.</param>
        /// <param name="y">The y postion in pixels to draw the text.</param>
        /// <param name="text">The text.</param>
        /// <returns>The size of the rendered text in pixels.</returns>
        public void Render(int x, int y, string text, TextOptions options)
        {
            int scalingFactor = ScalingFactor(options.FontSize);
            var glyphs = Layout(x, y, text, options).ToList();
            
            foreach (var layout in glyphs)
            {
                RenderGlyph(layout, scalingFactor);
            }
            _rasterizer.Flush();            
        }

        /// <summary>
        /// Calcualtes the scaling factor for the provided fontsize.
        /// </summary>
        /// <param name="fontSize">Size of the font.</param>
        /// <returns></returns>
        private int ScalingFactor(int fontSize)
        {
            return _rasterizer.Resolution * fontSize;
        }

        /// <summary>
        /// Measures the specified text in pixels,
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The size of the text in pixels os though it was rendered.</returns>
        public Size Measure(string text, TextOptions options)
        {
            var glyphs = Layout(0, 0, text, options);

            int scalingFactor = ScalingFactor(options.FontSize);

            var right = glyphs.Max(x => x.BottomRight.X);
            var left = glyphs.Min(x => x.TopLeft.X);
            var bottom = glyphs.Max(x => x.BottomRight.Y);
            var top = glyphs.Min(x => x.TopLeft.Y);

            return new Size()
            {
                Width = ((right - left) * scalingFactor) / FontToPixelDivisor,
                Height = ((bottom - top) * scalingFactor) / FontToPixelDivisor
            };
        }

        /// <summary>
        /// Layouts the <paramref name="text"/> at point specified by <paramref name="x"/> and <paramref name="y"/>
        /// based on the <paramref name="TextOptions"/>.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="text">The text.</param>
        /// <param name="options">The options.</param>
        /// <returns>Returns a collection of <see cref="GlyphLayout"/> that describe the layout positioning of each glyph.</returns>
        private IEnumerable<GlyphLayout> Layout(int x, int y, string text, TextOptions options)
        {
            //we are working a font sizes in here
            //convert from pixel sizes to font sizes
            int scalingFactor = ScalingFactor(options.FontSize);
            int xx = (x * FontToPixelDivisor) / scalingFactor;
            int yy = (y * FontToPixelDivisor) / scalingFactor;
            int startXX = xx;

            int lineHeight = (int)Math.Round(_typeface.LineSpacing * options.LineHeight);

            foreach (var character in text)
            {
                switch (character)
                {
                    case '\r':
                        // carrage return resets the XX cordinate to startXX 
                        xx = startXX;
                        break;
                    case '\n':
                        // newline/line feed resets the XX cordinate to startXX  
                        // and add a line height to the current YY
                        xx = startXX;
                        yy += lineHeight;
                        break;
                    default:
                        var glyph = _typeface.Lookup(character);
                        var glyphWidth = _typeface.GetAdvanceWidth(character);
                        // remove the min for EM square to calculate the final 'height' for the glyph from the origin because fonts need flipping to work sensibly.
                        int drawheightEM = EmSquare.Size - glyph.Bounds.YMin; 
                        yield return new GlyphLayout
                        {
                            glyph = glyph,
                            TopLeft = new Point<int>(xx, yy),
                            BottomRight = new Point<int>(glyphWidth + xx, drawheightEM + yy)
                        };

                        xx += glyphWidth;
                        break;
                }
            }
        }

        internal struct GlyphLayout
        {
            public Glyph glyph;
            public Point<int> TopLeft;
            public Point<int> BottomRight;
        }
    }
}

