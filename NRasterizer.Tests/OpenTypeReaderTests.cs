using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRasterizer.Tests
{

    [TestFixture]
    public class OpenTypeReaderTests
    {
        private string rootFolder;

        [SetUp]
        public void Setup()
        {
            this.rootFolder = Path.GetDirectoryName(new Uri(typeof(OpenTypeReaderTests).Assembly.Location).LocalPath);
        }

        public static IEnumerable<string> AllFonts => new[] { "OpenSans-Regular.ttf", "segoeui.ttf", "CompositeMS.ttf" };

        [Test]
        [TestCaseSource("AllFonts")]
        public void LoadingFontReturnsATypeface(string filename)
        {
            var reader = new OpenTypeReader();
            using (var fs = File.OpenRead($"{rootFolder}/TestFonts/{filename}"))
            {
                var typeface = reader.Read(fs);

                Assert.IsNotNull(typeface);
            }
        }
    }
}
