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

        private static void Grayscale(Image image)
        {
            var palette = image.Palette;
            for (int i = 0; i < palette.Entries.Length; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            image.Palette = palette;
        }

        private void DrawSoft(FileInfo fontPath, FileInfo target, string text)
        {
            const int width = 200;
            const int height = 80;
            const int resolution = 72;
            var options = new TextOptions()
            {
                FontSize = 64
            };
            var raster = new Raster(width, height, width, resolution);

            using (var input = fontPath.OpenRead())
            {
                var typeface = new OpenTypeReader().Read(input);
                var rasterizer = new Rasterizer.Rasterizer(raster);
                var renderer = new Renderer(typeface, rasterizer);
                renderer.Render(0, 0, text, options);
            }

            using (Bitmap b = new Bitmap(width, height, PixelFormat.Format8bppIndexed))
            {
                b.SetResolution(resolution, resolution);
                Grayscale(b);
                BlitTo(raster, b);
                b.Save(target.FullName, ImageFormat.Png);
            }
        }

        private void DrawGDI(FileInfo fontPath, FileInfo target, string text, bool drawbox)
        {
            const int width = 200;
            const int height = 80;
            const int resolution = 72;
            var options = new TextOptions()
            {
                FontSize = 64
            };
            int x = 0;
            int y = 0;

            using (var input = fontPath.OpenRead())
            {
                var typeface = new OpenTypeReader().Read(input);
                using (Bitmap b = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    b.SetResolution(resolution, resolution);
                    using (var g = Graphics.FromImage(b))
                    {
                        g.Clear(Color.White);
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        var rasterizer = new GDIGlyphRasterizer(g, resolution, Brushes.Black);
                        var renderer = new Renderer(typeface, rasterizer);
                        renderer.Render(x, y, text, options);

                        if (drawbox)
                        {
                            var size = renderer.Measure(text, options);
                            g.DrawRectangle(Pens.HotPink, new Rectangle(x, y, size.Width, size.Height));
                        }
                    }
                    b.Save(target.FullName, ImageFormat.Png);
                }
            }
        }

        public void Draw(string rasterizerName, FileInfo fontPath, FileInfo target, string text, bool drawbox)
        {
            target.Directory.Create();
            if (rasterizerName == "gdi+")
            {
                DrawGDI(fontPath, target, text, drawbox);
                return;
            }
            if (rasterizerName == "nrasterizer")
            {
                DrawSoft(fontPath, target, text);
                return;
            }
            throw new ApplicationException("Unknown rasterizer: " + rasterizerName);
        }

        private static string UnescapeWhitespace(string str)
        {
            return str
                .Replace("\\n", "\n")
                .Replace("\\t", "\t");
        }

        public static void Main(string[] args)
        {
            var rasterizerName = args[0];
            var fontPath = new FileInfo(args[1]);
            var target = new FileInfo(args[2]);
            var text = UnescapeWhitespace(args[3]);

            // add box to end of cmd line to draw a box outlining the measured text.
            var drawbox = false;
            if(args.Length > 4)
            {
                drawbox = args[4].Equals("box", StringComparison.OrdinalIgnoreCase);
            }
          
            var program = new NRasterizerProgram();
            program.Draw(rasterizerName, fontPath, target, text, drawbox);
        }
    }
}
