//Apache2, 2014-2016,   WinterDev
using System;

namespace NRasterizer
{
    public class Renderer
    {
        private const int PointsPerInch = 72;
        private readonly IGlyphRasterizer _rasterizer;
        private readonly Typeface _typeface;
        private const int pointsPerInch = 72;
        private const double FT_RESIZE = 64; //essential to be floating point

        public Renderer(Typeface typeface, IGlyphRasterizer rasterizer)
        {
            _typeface = typeface;
            _rasterizer = rasterizer;
        }

        public void RenderGlyph(int x, int y, int m, int d, Glyph glyph)
        {
            var rasterizer = new ToPixelRasterizer(x, y, m, d, _rasterizer);

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
                                                secondControlPoint.x,  secondControlPoint.y,
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

        private int ToPixels(int funits, int pointSize, int dpi)
        {
            return funits * pointSize * dpi / (EmSquare.Size * pointsPerInch);
        }

        public void Render(int x, int y, string text, int size, int resolution)
        {
            int xx = x * EmSquare.Size * PointsPerInch;
            int yy = y * EmSquare.Size * PointsPerInch;
            int m = size * resolution;
            int d = EmSquare.Size * PointsPerInch;
            foreach (var character in text)
            {
                RenderGlyph(xx, yy, m, d, _typeface.Lookup(character));
                xx += _typeface.GetAdvanceWidth(character);
            }
            _rasterizer.Flush();
        }
    }

}