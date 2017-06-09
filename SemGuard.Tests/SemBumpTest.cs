using System;
using b = SemBump.Bumper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SemGuard.Tests
{
    [TestClass]
    public class SemBumpTest
    {
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
            var thing = System.Text.Encoding.Default.GetString(Properties.Resources.SemGuard_Lib);
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
