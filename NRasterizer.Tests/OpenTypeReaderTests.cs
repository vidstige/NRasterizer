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
        [TestCase("CompositeMS.ttf")]
        [TestCase("segoeui.ttf")]
        [TestCase("OpenSans-Regular.ttf")]
        public void LoadingFontDoesntThrowAnyExceptions(string fontFilename)
        {
            var reader = new OpenTypeReader();
            Assert.DoesNotThrow(() =>
            {
                using (var fs = File.OpenRead($"TestFonts/{fontFilename}"))
                {
                    var typeface = reader.Read(fs);
                }
            });
        }
    }
}
