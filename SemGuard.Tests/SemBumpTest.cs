using System;
using b = SemBump.Bumper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.Linq;

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
      <package>
         <metadata >
           <id > choconuspec </id >
           <version > 3.0.0 </version >
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

            var nuget = @"<?xml version=""1.0"" encoding=""utf - 8""?>
<!--Do not remove this test for UTF - 8: if “Ω” doesn’t appear as greek uppercase omega letter enclosed in quotation marks, you should use an editor that supports UTF - 8, not this one. -->
      <package>
         <metadata >
           <id > nugetnuspec </id >
           <version > 1.3.3.7 </version >
     </metadata >
  
      </package >
      ";

            var result = b.BumpNuspecContents(choco, b.UnionOperation.Minor);
            var otherresult = b.BumpNuspecContents(nuget, b.UnionOperation.Major);


            Assert.IsTrue(result.Contains("3.1.0"));
            Assert.IsTrue(otherresult.Contains("2.0.0.0"));


            Assert.IsTrue(result.Contains("3.1.0<"), "Must maintain type of version");
            Assert.IsTrue(otherresult.Contains("2.0.0.0<"), "Must maintain type of version");
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
        [TestCategory("unit")]
        [ExpectedException(typeof(FormatException))]
        public void TestWildCardsSemantic()
        {
            var v = new SemVer.SemanticVersion("1.0.*");
        }

        [TestMethod]
        [TestCategory("unit")]
        [ExpectedException(typeof(FormatException))]
        public void TestWildCardSystem()
        {
            var v = new System.Version("1.0.*");
        }

        [TestMethod]
        public void BumpNuspecContentsTest()
        {
            var thing = System.Text.Encoding.Default.GetString(Properties.Resources.nuget);
            var actual = b.BumpNuspecContents(thing, b.UnionOperation.Major);
        }

        [TestMethod]
        public void TestPatch()
        {
            var original = "1.0.0";
            var expected = "1.0.1";
            var actual = b.Bump(original, b.UnionOperation.Patch).ToString();
            Assert.AreEqual(expected, actual, "Bump must properly increment patch value");

            actual = b.Bump(original, b.UnionOperation.Patch).ToString();
            Assert.AreEqual(expected, actual, "Bump must ignore case");
        }

        [TestMethod]
        public void TestParse()
        {
            Assert.AreEqual(b.UnionOperation.Patch, b.parseOperation("PaTcH"), "Function parseOperation must ignore case");
            Assert.AreEqual(b.UnionOperation.Build, b.parseOperation("BuilD"), "Function parseOperation must ignore case");
            Assert.AreEqual(b.UnionOperation.Minor, b.parseOperation("Minor"), "Function parseOperation must ignore case");
            Assert.AreEqual(b.UnionOperation.Major, b.parseOperation("Major"), "Function parseOperation must ignore case");
        }

        [TestMethod]
        [TestCategory("unit")]
        public void TestMinor()
        {
            var original = "1.0.0";
            var expected = "1.1.0";
            var actual = b.Bump(original, b.UnionOperation.Minor).ToString();
            Assert.AreEqual(expected, actual, "Bump must properly increment minor value");
        }

        [TestMethod]
        [TestCategory("unit")]
        public void TestMajor()
        {
            var original = "1.0.0";
            var expected = "2.0.0";
            var actual = b.Bump(original, b.UnionOperation.Major).ToString();
            Assert.AreEqual(expected, actual, "Bump must properly increment major value");
        }

        [TestMethod]
        [TestCategory("unit")]
        public void TestInvalidOperator()
        {
            Assert.IsNull(b.parseOperation("invalid"), "Function parseOperation must return none type (null in C# land)");
        }
    }
}
