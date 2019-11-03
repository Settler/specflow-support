using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Tests
{
    [TestFileExtension(GherkinProjectFileType.FEATURE_EXTENSION)]
    public class GherkinParserTest : ParserTestBase<GherkinLanguage>
    {
        protected override string RelativeTestDataPath => "Parsing";
        
        [TestCase("Features")]
        public void TestParser(string name) { DoOneTest(name); }
    }
}