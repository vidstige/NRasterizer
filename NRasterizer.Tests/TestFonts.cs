using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRasterizer.Tests
{
    public static class TestFonts
    {
        public static IEnumerable<string> AllFonts
        {
            get
            {
                var fontFiles = new[] { "OpenSans-Regular.ttf", "segoeui.ttf", "CompositeMS.ttf" };
                var rootFolder = Path.GetDirectoryName(new Uri(typeof(TestFonts).Assembly.Location).LocalPath);
                foreach (var fontFile in fontFiles)
                {
                    yield return $"{rootFolder}/TestFonts/{fontFile}";
                }
            }
        }
    }
}
