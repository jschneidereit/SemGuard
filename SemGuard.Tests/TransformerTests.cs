using Microsoft.CodeAnalysis;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using Xunit;
using SemGuard;

namespace SemGuard.Tests
{
    public class TransformerTests
    {
        //TODO: point these to the embedded resources
        private IEnumerable<(Topology, Compilation, Project)> Base(string temp) => Transformer.LoadSolution(Path.Combine(temp, @"BaseDummy\Dummy.sln")).ToList();
        private (Topology, Compilation, Project) Major(string temp) => Transformer.LoadSolution(Path.Combine(temp, @"MajorDummy\Dummy.sln")).First();
        private (Topology, Compilation, Project) Minor(string temp) => Transformer.LoadSolution(Path.Combine(temp, @"MinorDummy\Dummy.sln")).First();
        private (Topology, Compilation, Project) Patch(string temp) => Transformer.LoadSolution(Path.Combine(temp, @"PatchDummy\Dummy.sln")).First();

        private static string GetTemporaryDirectory()
        {
            var systemTemp = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
            var temp = Path.Combine(systemTemp, "semguard", Guid.NewGuid().ToString());
            Directory.CreateDirectory(temp);
            return temp;
        }

        private static string GetNewTempEnvironment()
        {
            var target = new DirectoryInfo(GetTemporaryDirectory());
            var source = new DirectoryInfo(@"..\..\Dummy");

            CopyTestFilesRec(source, target);

            return target.FullName;
        }

        /// <summary>
        /// Thanks stack overflow: https://stackoverflow.com/a/58779
        /// </summary>
        private static void CopyTestFilesRec(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories())
            {
                CopyTestFilesRec(dir, target.CreateSubdirectory(dir.Name));
            }

            foreach (var file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }

        private static bool SetEquals<T>(List<T> left, List<T> right) where T : IComparable
        {
            return left.TrueForAll(obj => right.Contains(obj)) && right.TrueForAll(m => left.Contains(m));
        }
        
        public void Cleanup()
        {
            try
            {
                Directory.Delete(Path.Combine(Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine), "semguard"), true);
            }
            catch
            {
                //don't really care if this blows up, I tried :P
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetNewAssemblyVersionTest()
        {
            var temp = GetNewTempEnvironment();

            var minor = Minor(temp);
            var major = Major(temp);
            var patch = Patch(temp);
            var b = Base(temp);

            var majorchange = major.Item1.DetermineSemanticChange(b.First().Item1);
            var minorchange = minor.Item1.DetermineSemanticChange(b.First().Item1);
            var patchchange = patch.Item1.DetermineSemanticChange(b.First().Item1);

            Assert.Equal(new Version("2.0.0.0"), major.Item1.GetNewAssemblyVersion(majorchange));
            Assert.Equal(new Version("1.1.0.0"), minor.Item1.GetNewAssemblyVersion(minorchange));
            Assert.Equal(new Version("1.0.1.0"), patch.Item1.GetNewAssemblyVersion(patchchange));

            Directory.Delete(temp, true);
        }

        [Fact]
        public void GetNewAssemblyVersionDefaultTest()
        {
            var actual = default(SemanticChange);
            Assert.Equal(SemanticChange.Patch, actual);
        }

        [Fact]
        public void DefaultVersionTest()
        {
            Assert.Equal(new Version("1.0.0.0"), Transformer.DefaultVersion());
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetCurrentVersionTest()
        {
            var temp = GetNewTempEnvironment();

            var compilation = Base(temp).First().Item2;
            var actual = compilation.GetCurrentVersion();
            Assert.Equal(new Version("1.0.0.0"), actual);

            Directory.Delete(temp, true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetAssemblyNameTest()
        {
            var temp = GetNewTempEnvironment();

            var compilation = Base(temp).First().Item2;
            var actual = compilation.GetAssemblyName();
            Assert.Equal("Dummy", actual);

            Directory.Delete(temp, true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetAssemblyFilePathTest()
        {
            var temp = GetNewTempEnvironment();

            var expected = Path.Combine(temp, @"\BaseDummy\Dummy.csproj");
            var actual = Base(temp).First().Item3.GetAssemblyFilePath();
            Assert.Equal(expected, actual);

            Directory.Delete(temp, true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetTopoFilePathTest()
        {
            var temp = GetNewTempEnvironment();

            var expected = Path.Combine(temp, @"BaseDummy\Dummy.csproj.topo");
            var actual = Base(temp).First().Item1.GetTopoFilePath();
            Assert.Equal(expected, actual);

            Directory.Delete(temp, true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TopoFileExistsTest()
        {
            var temp = GetNewTempEnvironment();
            var topo = Path.Combine(temp, @"BaseDummy\Dummy.csproj.topo");
            var b = Base(temp);

            //if (!File.Exists(topo))
            //{
            //    b.First().Item1.SaveToFile();
            //}

            Assert.True(Transformer.TopoFileExists(b.First().Item1.GetTopoFilePath()));
            Assert.False(Transformer.TopoFileExists(Path.Combine(temp, @"BaseDummy\Dummy.csproj")));

            Directory.Delete(temp, true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void BuildTopologyAndLoadFromFileTest()
        {
            var temp = GetNewTempEnvironment();
            var path = Path.Combine(temp, @"BaseDummy\Dummy.csproj.topo");
            var b = Base(temp);
            if (!File.Exists(path))
            {
                b.First().Item1.SaveToFile();
            }

            var fromfile = Transformer.LoadFromFile(path);
            var fromassembly = Transformer.BuildTopology(b.First().Item2, b.First().Item3);
            Assert.Equal(fromassembly.AssemblyName, fromfile.AssemblyName);
            Assert.Equal(fromassembly.AssemblyPath, fromfile.AssemblyPath);
            Assert.True(SetEquals(fromassembly.AssemblyReferences, fromfile.AssemblyReferences));
            Assert.True(SetEquals(fromassembly.PublicApi, fromfile.PublicApi));
            Assert.Equal(fromassembly.Version, fromfile.Version);

            Directory.Delete(temp, true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void BuildTopologyTestNullProject()
        {
            var temp = GetNewTempEnvironment();

            Assert.Throws<ArgumentNullException>(() => Transformer.BuildTopology(null, Base(temp).First().Item3));
            Assert.Throws<ArgumentNullException>(() => Transformer.BuildTopology(Base(temp).First().Item2, null));

            Directory.Delete(temp, true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SaveToFileTest()
          {
            var temp = GetNewTempEnvironment();
            var path = Path.Combine(temp, @"BaseDummy\Dummy.csproj.topo");
            var b = Base(temp);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            b.First().Item1.SaveToFile();
            Assert.True(Transformer.TopoFileExists(b.First().Item1.GetTopoFilePath()));

            Directory.Delete(temp, true);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void AdditionsTest()
        {
            var old = new List<int>() { 1, 2, 3 };
            var newrem = new List<int>() { 1, 2 };
            var newboth = new List<int>() { 1, 2, 4, 5 };
            var newadd = new List<int>() { 1, 2, 3, 4 };

            Assert.False(Transformer.Additions(newrem, old));
            Assert.True(Transformer.Additions(newboth, old));
            Assert.True(Transformer.Additions(newadd, old));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void AdditionNullLists()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                List<int> x = null;
                List<int> y = null;

                Transformer.Additions(x, y);
            });
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RemovalNullLists()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                List<int> x = null;
                List<int> y = null;

                Transformer.Removals(x, y);
            });
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RemovalsTest()
        {
            var old = new List<int>() { 1, 2, 3 };
            var newrem = new List<int>() { 1, 2 };
            var newboth = new List<int>() { 1, 2, 4, 5 };
            var newadd = new List<int>() { 1, 2, 3, 4 };

            Assert.True(Transformer.Removals(newrem, old));
            Assert.True(Transformer.Removals(newboth, old));
            Assert.False(Transformer.Removals(newadd, old));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void DetermineSemanticChangeTest()
        {
            var temp = GetNewTempEnvironment();

            var minor = Minor(temp);
            var major = Major(temp);
            var patch = Patch(temp);
            var b = Base(temp);

            //TODO: figure this out later http://stackoverflow.com/a/65062/1895962
            Assert.Equal(SemanticChange.Minor, minor.Item1.DetermineSemanticChange(b.First().Item1));
            Assert.Equal(SemanticChange.Major, major.Item1.DetermineSemanticChange(b.First().Item1));
            Assert.Equal(SemanticChange.Patch, patch.Item1.DetermineSemanticChange(b.First().Item1));

            Directory.Delete(temp);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GetAssemblyReferencesNullTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Compilation c = null;
                c.GetAssemblyReferences();
            });
        }

        [Fact]
        public void SetAssemblyVersionCreatesTopoTest()
        {
            var temp = GetNewTempEnvironment();
            var b = Base(temp);
            var path = b.First().Item1.GetTopoFilePath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            try
            {
                b.First().Item2.UpdateAssemblyVersion(b.First().Item1, SemanticChange.Patch);
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
            Assert.True(File.Exists(path));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetAssemblyReferenceTest()
        {
            var temp = GetNewTempEnvironment();
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
            var references = Base(temp).First().Item2.GetAssemblyReferences();
            Assert.True(SetEquals(expected, references));
        }
    }
}