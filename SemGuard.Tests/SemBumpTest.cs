using System;
using b = SemBump.Bumper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;

namespace SemGuard.Tests
{
    [TestClass]
    public class SemBumpTest
    {
        [TestMethod]
        public void BumpChocoTest()
        {
            var choco = @"<?xml version=""1.0"" encoding=""utf - 8""?>
<!--Do not remove this test for UTF - 8: if “Ω” doesn’t appear as greek uppercase omega letter enclosed in quotation marks, you should use an editor that supports UTF - 8, not this one. -->
      <package xmlns = ""http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd"" >
         <metadata >
           <id > choconuspec </id >
           <version > 1.3.3.7 </version >
              <title > choconuspec(Install) </title >
              <authors > __REPLACE_AUTHORS_OF_SOFTWARE_COMMA_SEPARATED__ </authors >
              <projectUrl > https://_Software_Location_REMOVE_OR_FILL_OUT_</projectUrl>
    <tags > choconuspec admin SPACE_SEPARATED</tags >
       <summary > __REPLACE__ </summary >
       <description > __REPLACE__MarkDown_Okay </description >
     </metadata >
     <files >
       <file src = ""tools\**"" target = ""tools"" />
        </files >
      </package >
      ";

           

            var blah = XDocument.Parse(choco);
            var a = blah.Root.Element(XName.Get("package"));

            var result = b.BumpNuspecContents(choco, "major");

        }

        [TestMethod]
        public void DetermineVersionTypeTest()
        {
            var test_version = "1.0.0-rc1";

            try
            {   
                var semv = new SemVer.SemanticVersion(test_version);
            }
            catch (Exception)
            {
                try
                {
                    var sysv = new System.Version(test_version);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [TestMethod]
        public void BumpNuspecContentsTest()
        {
            var thing = System.Text.Encoding.Default.GetString(Properties.Resources.nuget);
            var actual = b.BumpNuspecContents(thing, "major");
        }

        [TestMethod]
        public void TestPatch()
        {
            var original = "1.0.0";
            var expected = "1.0.1";
            var actual = b.Bump(original, "patch").ToString();
            Assert.AreEqual(expected, actual, "Bump must properly increment patch value");

            actual = b.Bump(original, "PatcH").ToString();
            Assert.AreEqual(expected, actual, "Bump must ignore case");
        }

        [TestMethod]
        public void TestMinor()
        {
            var original = "1.0.0";
            var expected = "1.1.0";
            var actual = b.Bump(original, "minor").ToString();
            Assert.AreEqual(expected, actual, "Bump must properly increment minor value");

            actual = b.Bump(original, "MiNoR").ToString();
            Assert.AreEqual(expected, actual, "Bump must ignore case");
        }

        [TestMethod]
        public void TestMajor()
        {
            var original = "1.0.0";
            var expected = "2.0.0";
            var actual = b.Bump(original, "major").ToString();
            Assert.AreEqual(expected, actual, "Bump must properly increment major value");

            actual = b.Bump(original, "MAjOr").ToString();
            Assert.AreEqual(expected, actual, "Bump must ignore case");
        }

        [TestMethod]
        public void TestInvalidOperator()
        {
            var original = "1.0.0";
            var expected = "1.0.0";
            var actual = b.Bump(original, "notvalid").ToString();
            Assert.AreEqual(expected, actual, "Invalid operators return the same version");
        }
    }
}
