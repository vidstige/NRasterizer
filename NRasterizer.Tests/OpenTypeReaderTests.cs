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
        [TestCaseSource(typeof(TestFontsPaths), "AllFonts")]
        public void LoadingFontDoesntThrowAnyExceptions(string path)
        {
            var reader = new OpenTypeReader();
            Assert.DoesNotThrow(() =>
            {
                using (var fs = File.OpenRead(path))
                {
                    reader.Read(fs);
                }
            });
        }
    }
}
