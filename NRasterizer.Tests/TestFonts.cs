using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRasterizer.Tests
{
    public static class TestFontsPaths
    {
        private static readonly string rootFolder;

        static TestFontsPaths()
        {
            rootFolder =  Path.GetDirectoryName( new Uri(typeof(TestFonts).Assembly.Location).LocalPath);
        }

        public static string OpenSans_Regular => $"{rootFolder}/TestFonts/OpenSans-Regular.ttf";
        public static string SegoeUI => $"{rootFolder}/TestFonts/segoeui.ttf";
        public static string CompositeMS => $"{rootFolder}/TestFonts/CompositeMS.ttf";

        public static IEnumerable<string> AllFonts
        {
            get
            {
                yield return OpenSans_Regular;
                yield return SegoeUI;
                yield return CompositeMS;
            }
        }
    }
    
    public static class TestFonts
    {
        private static readonly string rootFolder;
        private static OpenTypeReader reader = new OpenTypeReader();
        private static Dictionary<string, Typeface> cache = new Dictionary<string, Typeface>();

        private static Typeface Load(string path)
        {
            if (cache.ContainsKey(path))
            {
                return cache[path];
            }
            lock (cache) {
                if (cache.ContainsKey(path))
                {
                    return cache[path];
                }
                using (var fs = File.OpenRead(path))
                {
                    cache.Add(path, reader.Read(fs));
                }
            }

            return cache[path];
        }


        public static Typeface OpenSans_Regular => Load(TestFontsPaths.OpenSans_Regular);
        public static Typeface SegoeUI => Load(TestFontsPaths.SegoeUI);
        public static Typeface CompositeMS => Load(TestFontsPaths.CompositeMS);
    }
}
