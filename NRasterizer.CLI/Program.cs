using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NRasterizer.Rasterizer;

namespace NRasterizer.CLI
{
    public class NRasterizerProgram
    {
        private static Rectangle Entire(Bitmap b)
        {
            return new Rectangle(0, 0, b.Width, b.Height);
        }

        private static void BlitTo(Raster raster, Bitmap bitmap)
        {
            var b = bitmap;
            BitmapData bitmapData = null;
            try
            {
                bitmapData = b.LockBits(Entire(b), ImageLockMode.WriteOnly, b.PixelFormat);
                int si = 0;
                IntPtr di = bitmapData.Scan0;
                for (int y = 0; y < b.Height; y++)
                {
                    Marshal.Copy(raster.Pixels, si, di, b.Width);
                    si += raster.Stride;
                    di = new IntPtr(di.ToInt64() + bitmapData.Stride);
                }
            }
            finally
            {
                if (bitmapData != null)
                {
                    b.UnlockBits(bitmapData);
                }
            }
        }

        private void DrawSoft(FileInfo fontPath, FileInfo target, string text)
        {
            const int width = 200;
            const int height = 80;
            var raster = new Raster(width, height, width, 72);

            using (var input = fontPath.OpenRead())
            {
                var typeface = new OpenTypeReader().Read(input);
                var rasterizer = new Rasterizer.Rasterizer(typeface, raster);
                var renderer = new Renderer(typeface, rasterizer);
                renderer.Render(0, 0, text, 36, 72);
            }

            using (Bitmap b = new Bitmap(width, height, PixelFormat.Format32bppPArgb))
            {
                BlitTo(raster, b);
                b.Save(target.FullName, ImageFormat.Png);
            }
        }

        void DrawGDI(FileInfo fontPath, FileInfo target, string text)
        {
            const int width = 200;
            const int height = 80;

            using (var input = fontPath.OpenRead())
            {
                var typeface = new OpenTypeReader().Read(input);
                using (Bitmap b = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    using (var g = Graphics.FromImage(b))
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        var rasterizer = new GDIGlyphRasterizer(g);
                        var renderer = new Renderer(typeface, rasterizer);
                        renderer.Render(0, 0, text, 36, 72);
                    }
                    b.Save(target.FullName, ImageFormat.Png);
                }

            }
        }

        public static void Main(string[] args)
        {
            var fontPath = new FileInfo(args[0]);
            var target = new FileInfo(args[1]);
            var text = args[2];

            var program = new NRasterizerProgram();
            target.Directory.Create();
            program.DrawGDI(fontPath, target, text);
            //program.DrawSoft(fontPath, target, text);
        }
    }
}
