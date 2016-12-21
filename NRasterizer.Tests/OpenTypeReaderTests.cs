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
        [Test]
        [TestCaseSource(typeof(TestFonts), "AllFonts")]
        public void LoadingFontReturnsATypeface(string path)
        {
            var reader = new OpenTypeReader();
            using (var fs = File.OpenRead(path))
            {
                var typeface = reader.Read(fs);

                Assert.IsNotNull(typeface);
            }
        }
    }
}
