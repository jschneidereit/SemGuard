using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using SemGuard.Lib;
using SemGuard.Tests.Properties;

namespace SemGuard.Tests
{
    [TestClass()]
    public class TransformerTests
    {
        //TODO: point these to the embedded resources
        private IEnumerable<(Topology, Compilation, Project)> Base { get; } = Transformer.LoadSolution(@"..\Dummy\BaseDummy\Dummy.sln").ToList();
        private (Topology, Compilation, Project) Major { get; } = Transformer.LoadSolution(@"..\Dummy\MajorDummy\Dummy.sln").First();
        private (Topology, Compilation, Project) Minor { get; } = Transformer.LoadSolution(@"..\Dummy\MinorDummy\Dummy.sln").First();
        private (Topology, Compilation, Project) Patch { get; } = Transformer.LoadSolution(@"..\Dummy\PatchDummy\Dummy.sln").First();

        private static bool SetEquals<T>(List<T> left, List<T> right) where T : IComparable
        {
            return left.TrueForAll(obj => right.Contains(obj)) && right.TrueForAll(m => left.Contains(m));
        }

        [TestMethod()]
        public void GetNewAssemblyVersionTest()
        {
            var majorchange = Major.Item1.DetermineSemanticChange(Base.First().Item1);
            var minorchange = Minor.Item1.DetermineSemanticChange(Base.First().Item1);
            var patchchange = Patch.Item1.DetermineSemanticChange(Base.First().Item1);

            Assert.AreEqual(new Version("2.0.0.0"), Major.Item1.GetNewAssemblyVersion(majorchange));
            Assert.AreEqual(new Version("1.1.0.0"), Minor.Item1.GetNewAssemblyVersion(minorchange));
            Assert.AreEqual(new Version("1.0.1.0"), Patch.Item1.GetNewAssemblyVersion(patchchange));
        }

        [TestMethod()]
        public void GetNewAssemblyVersionDefaultTest()
        {
            Assert.AreEqual(SemanticChange.Patch, default(SemanticChange));
        }

        [TestMethod()]
        public void DefaultVersionTest()
        {
            Assert.AreEqual(new Version("1.0.0.0"), Transformer.DefaultVersion());
        }

        [TestMethod()]
        public void GetCurrentVersionTest()
        {
            var compilation = Base.First().Item2;
            var actual = compilation.GetCurrentVersion();
            Assert.AreEqual(new Version("1.0.0.0"), actual);
        }

        [TestMethod()]
        public void GetAssemblyNameTest()
        {
            var compilation = Base.First().Item2;
            var actual = compilation.GetAssemblyName();
            Assert.AreEqual("Dummy", actual);
        }

        [TestMethod()]
        public void GetAssemblyFilePathTest()
        {
            var expected = @"C:\Repos\SemVerRecorder\SemVerRecorder.Tests\Dummy\BaseDummy\Dummy.csproj";
            var actual = Base.First().Item3.GetAssemblyFilePath();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetTopoFilePathTest()
        {
            var expected = @"C:\Repos\SemVerRecorder\SemVerRecorder.Tests\Dummy\BaseDummy\Dummy.csproj.topo";
            var actual = Base.First().Item1.GetTopoFilePath();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TopoFileExistsTest()
        {
            if (!File.Exists(@"C:\Repos\SemVerRecorder\SemVerRecorder.Tests\Dummy\BaseDummy\Dummy.csproj.topo"))
            {
                Base.First().Item1.SaveToFile();
            }

            Assert.IsTrue(Transformer.TopoFileExists(Base.First().Item1.GetTopoFilePath()));
            Assert.IsFalse(Transformer.TopoFileExists(@"C:\Repos\SemVerRecorder\SemVerRecorder.Tests\Dummy\BaseDummy\Dummy.csproj"));
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void BuildTopologyAndLoadFromFileTest()
        {
            var path = @"C:\Repos\SemVerRecorder\SemVerRecorder.Tests\Dummy\BaseDummy\Dummy.csproj.topo";
            if (!File.Exists(path))
            {
                Base.First().Item1.SaveToFile();
            }

            var fromfile = Transformer.LoadFromFile(path);
            var fromassembly = Transformer.BuildTopology(Base.First().Item2, Base.First().Item3);
            Assert.AreEqual(fromassembly.AssemblyName, fromfile.AssemblyName);
            Assert.AreEqual(fromassembly.AssemblyPath, fromfile.AssemblyPath);
            Assert.IsTrue(SetEquals(fromassembly.AssemblyReferences, fromfile.AssemblyReferences));
            Assert.IsTrue(SetEquals(fromassembly.PublicApi, fromfile.PublicApi));
            Assert.AreEqual(fromassembly.Version, fromfile.Version);
        }

        [TestMethod()]
        [TestCategory("Integration")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildTopologyTestNullCompilation()
        {
            Transformer.BuildTopology(null, Base.First().Item3);
        }

        [TestMethod()]
        [TestCategory("Integration")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildTopologyTestNullProject()
        {
            Transformer.BuildTopology(Base.First().Item2, null);
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void SaveToFileTest()
        {
            var path = @"C:\Repos\SemVerRecorder\SemVerRecorder.Tests\Dummy\BaseDummy\Dummy.csproj.topo";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Base.First().Item1.SaveToFile();
            Assert.IsTrue(Transformer.TopoFileExists(Base.First().Item1.GetTopoFilePath()));
        }

        [TestMethod()]
        public void AdditionsTest()
        {
            var old = new List<int>() { 1, 2, 3 };
            var newrem = new List<int>() { 1, 2 };
            var newboth = new List<int>() { 1, 2, 4, 5 };
            var newadd = new List<int>() { 1, 2, 3, 4 };

            Assert.IsFalse(Transformer.Additions(newrem, old));
            Assert.IsTrue(Transformer.Additions(newboth, old));
            Assert.IsTrue(Transformer.Additions(newadd, old));
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdditionNullLists()
        {
            List<int> x = null;
            List<int> y = null;

            Transformer.Additions(x, y);
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemovalNullLists()
        {
            List<int> x = null;
            List<int> y = null;

            Transformer.Removals(x, y);
        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void RemovalsTest()
        {
            var old = new List<int>() { 1, 2, 3 };
            var newrem = new List<int>() { 1, 2 };
            var newboth = new List<int>() { 1, 2, 4, 5 };
            var newadd = new List<int>() { 1, 2, 3, 4 };

            Assert.IsTrue(Transformer.Removals(newrem, old));
            Assert.IsTrue(Transformer.Removals(newboth, old));
            Assert.IsFalse(Transformer.Removals(newadd, old));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void DetermineSemanticChangeTest()
        {
            //TODO: figure this out later http://stackoverflow.com/a/65062/1895962
            Assert.AreEqual(SemanticChange.Minor, Minor.Item1.DetermineSemanticChange(Base.First().Item1));
            Assert.AreEqual(SemanticChange.Major, Major.Item1.DetermineSemanticChange(Base.First().Item1));
            Assert.AreEqual(SemanticChange.Patch, Patch.Item1.DetermineSemanticChange(Base.First().Item1));
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetAssemblyReferencesNullTest()
        {
            Compilation c = null;
            c.GetAssemblyReferences();
        }

        [TestMethod()]
        public void SetAssemblyVersionCreatesTopoTest()
        {
            var path = Base.First().Item1.GetTopoFilePath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            try
            {
                Base.First().Item2.UpdateAssemblyVersion(Base.First().Item1, SemanticChange.Patch);
            }
            catch (NotImplementedException)
            {
                //TODO: Fix this when I finish implementing this
            }
            catch (Exception)
            {
                throw;
            }

            //TODO: determine if there is a way to tell git to ignore changes to the topo and csproj test files
            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GetAssemblyReferenceTest()
        {
            var expected = new List<(string, Version)> {
                ("Microsoft.CSharp", new Version("4.0.0.0")),
                ("mscorlib", new Version("4.0.0.0")),
                ("System.Core",new Version("4.0.0.0")),
                ("System.Data.DataSetExtensions", new Version("4.0.0.0")),
                ("System.Data",new Version("4.0.0.0")),
                ("System", new Version("4.0.0.0")),
                ("System.Net.Http",new Version("4.0.0.0")),
                ("System.Xml",new Version("4.0.0.0")),
                ("System.Xml.Linq",new Version("4.0.0.0")) };
            var references = Base.First().Item2.GetAssemblyReferences();
            Assert.IsTrue(SetEquals(expected, references));
        }
    }
}