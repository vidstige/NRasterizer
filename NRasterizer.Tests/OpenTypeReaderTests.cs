using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NRasterizer.Tests
{
    public class OpenTypeReaderTests
    {
        private string fontsRoot;

        public OpenTypeReaderTests()
        {
            // this might need to be conditional based on which test runner is running... i.e. appveyer might need a different path.
            var paths = new[] {
                "..\\Fonts", // ran from VS test explorer
                "Fonts" // ran from build script/ root direction with dotnet test NRasterizer.Tests
            };

            this.fontsRoot  = paths.Where(x => Directory.Exists(x)).First();
        }

        public static TheoryData<string> AllFonts => new TheoryData<string>() {
            "OpenSans-Regular.ttf",
            "segoeui.ttf",
            "CompositeMS.ttf"
        };

        [Theory]
        [MemberData(nameof(AllFonts))]
        public void LoadingFontReturnsATypeface(string filename)
        {
            var reader = new OpenTypeReader();
            using (var fs = File.OpenRead($"{fontsRoot}/{filename}"))
            {
                var typeface = reader.Read(fs);

                Assert.NotNull(typeface);
            }
        }
    }
}
