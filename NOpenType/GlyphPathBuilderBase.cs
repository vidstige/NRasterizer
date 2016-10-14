//Apache2, 2014-2016,   WinterDev
using System;
using System.Collections.Generic;
using System.Linq;

namespace NRasterizer
{
    public class GlyphPathBuilderBase
    {
        private readonly IGlyphRasterizer _rasterizer;
        readonly Typeface _typeface;
        const int pointsPerInch = 72;

        public GlyphPathBuilderBase(Typeface typeface, IGlyphRasterizer rasterizer)
        {
            _typeface = typeface;
            _rasterizer = rasterizer;
        }
        const double FT_RESIZE = 64; //essential to be floating point

        void RenderGlyph(ushort[] contours, FtPoint[] ftpoints, int[] flags)
        {

            //outline version
            //-----------------------------
            int npoints = ftpoints.Length;
            int startContour = 0;
            int cpoint_index = 0;
            int todoContourCount = contours.Length;
            //-----------------------------------
            _rasterizer.BeginRead(todoContourCount);
            //-----------------------------------
            double lastMoveX = 0;
            double lastMoveY = 0;

            int controlPointCount = 0;
            while (todoContourCount > 0)
            {
                int nextContour = contours[startContour] + 1;
                bool isFirstPoint = true;
                FtPointD secondControlPoint = new FtPointD();
                FtPointD thirdControlPoint = new FtPointD();
                bool justFromCurveMode = false;

                for (; cpoint_index < nextContour; ++cpoint_index)
                {
                    FtPoint vpoint = ftpoints[cpoint_index];
                    int vtag = (int)flags[cpoint_index] & 0x1;
                    //bool has_dropout = (((vtag >> 2) & 0x1) != 0);
                    //int dropoutMode = vtag >> 3;
                    if ((vtag & 0x1) != 0)
                    {
                        //on curve
                        if (justFromCurveMode)
                        {
                            switch (controlPointCount)
                            {
                                case 1:
                                    {
                                        _rasterizer.Curve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                            vpoint.X / FT_RESIZE, vpoint.Y / FT_RESIZE);
                                    }
                                    break;
                                case 2:
                                    {
                                        _rasterizer.Curve4(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                            thirdControlPoint.x / FT_RESIZE, thirdControlPoint.y / FT_RESIZE,
                                            vpoint.X / FT_RESIZE, vpoint.Y / FT_RESIZE);
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
                                _rasterizer.MoveTo(lastMoveX = (vpoint.X / FT_RESIZE), lastMoveY = (vpoint.Y / FT_RESIZE));
                            }
                            else
                            {
                                _rasterizer.LineTo(vpoint.X / FT_RESIZE, vpoint.Y / FT_RESIZE);
                            }

                            //if (has_dropout)
                            //{
                            //    //printf("[%d] on,dropoutMode=%d: %d,y:%d \n", mm, dropoutMode, vpoint.x, vpoint.y);
                            //}
                            //else
                            //{
                            //    //printf("[%d] on,x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                            //}
                        }
                    }
                    else
                    {
                        switch (controlPointCount)
                        {
                            case 0:
                                {   //bit 1 set=> off curve, this is a control point
                                    //if this is a 2nd order or 3rd order control point
                                    if (((vtag >> 1) & 0x1) != 0)
                                    {
                                        //printf("[%d] bzc3rd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        thirdControlPoint = new FtPointD(vpoint);
                                    }
                                    else
                                    {
                                        //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        secondControlPoint = new FtPointD(vpoint);
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (((vtag >> 1) & 0x1) != 0)
                                    {
                                        //printf("[%d] bzc3rd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        thirdControlPoint = new FtPointD(vpoint.X, vpoint.Y);
                                    }
                                    else
                                    {
                                        //we already have prev second control point
                                        //so auto calculate line to 
                                        //between 2 point
                                        FtPointD mid = GetMidPoint(secondControlPoint, vpoint);
                                        //----------
                                        //generate curve3
                                        _rasterizer.Curve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                            mid.x / FT_RESIZE, mid.y / FT_RESIZE);
                                        //------------------------
                                        controlPointCount--;
                                        //------------------------
                                        //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        secondControlPoint = new FtPointD(vpoint);
                                    }
                                }
                                break;
                            default:
                                {
                                    throw new NotSupportedException();
                                }
                                break;
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
                                _rasterizer.Curve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        case 2:
                            {
                                _rasterizer.Curve4(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                    thirdControlPoint.x / FT_RESIZE, thirdControlPoint.y / FT_RESIZE,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        default:
                            { throw new NotSupportedException(); }
                    }
                    justFromCurveMode = false;
                    controlPointCount = 0;
                }
                _rasterizer.CloseFigure();
                //--------                   
                startContour++;
                todoContourCount--;
            }
            _rasterizer.EndRead();
        }
        static FtPointD GetMidPoint(FtPoint v1, FtPoint v2)
        {
            return new FtPointD(
                ((double)v1.X + (double)v2.X) / 2d,
                ((double)v1.Y + (double)v2.Y) / 2d);
        }
        static FtPointD GetMidPoint(FtPointD v1, FtPointD v2)
        {
            return new FtPointD(
                ((double)v1.x + (double)v2.x) / 2d,
                ((double)v1.y + (double)v2.y) / 2d);
        }
        static FtPointD GetMidPoint(FtPointD v1, FtPoint v2)
        {
            return new FtPointD(
                (v1.x + (double)v2.X) / 2d,
                (v1.y + (double)v2.Y) / 2d);
        }

        void RenderGlyph(Glyph glyph)
        {
            var xs = glyph.X;
            var ys = glyph.Y;
            var ftpoints = new List<FtPoint>();
            for (int i = 0; i < xs.Length; i++)
            {
                ftpoints.Add(new FtPoint(xs[i], ys[i]));
            }
            var flags = glyph.On.Select(on => on ? 0 : 1).ToArray();
            RenderGlyph(glyph.EndPoints, ftpoints.ToArray(), flags);
        }

        public void Build(char c, int size, int resolution)
        {
            float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);
            RenderGlyph(_typeface.Lookup(c));
        }
    }

}