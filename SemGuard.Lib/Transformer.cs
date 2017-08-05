using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.MSBuild;

namespace SemGuard.Lib
{
    public static class Transformer
    {
        /// <summary>
        /// See http://semver.org/#spec-item-5, this is the first definition of an API.
        /// Currently not supporting a concept of build number.
        /// TODO: support a concept of build number, once this is setup it should be pretty easy to add a cmdline option to set build number
        /// </summary>
        public static Version DefaultVersion() => new Version("1.0.0.0");

        /// <summary>
        /// We expect this to be the type name for where the assembly version is defined in a standard C# project.
        /// </summary>
        public const string SYSTEM_REFLECTION_ASSEMBLY_VERSION = "System.Reflection.AssemblyVersionAttribute";

        public static List<(Topology, Compilation, Project)> LoadSolution(string solution)
        {
            var workspace = MSBuildWorkspace.Create();
            var sln = workspace.OpenSolutionAsync(solution).Result;
            var pairs = sln.Projects.Select(p => (p.GetCompilationAsync().Result.AddReferences(p.MetadataReferences), p));
            return pairs.Select(p => (BuildTopology(p.Item1, p.Item2), p.Item1, p.Item2)).ToList();
        }

        /// <summary>
        /// Gets the attribute for for the assembly version usually found in AssemblyInfo.cs.
        /// Searches for an attribute with name provided here <see cref="SYSTEM_REFLECTION_ASSEMBLY_VERSION"/>
        /// </summary>
        /// <param name="compilation">The compilation of the current assembly</param>
        /// <returns>The attribute containing the assembly version</returns>
        public static AttributeData GetAssemblyVersionAttribute(this Compilation compilation)
        {
            return compilation?.Assembly.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.ToString().Equals(SYSTEM_REFLECTION_ASSEMBLY_VERSION));
        }

        public static AttributeSyntax ToAttributeSyntax(this Version version, string attributeName = "AssemblyVersion")
        {
            var newname = SyntaxFactory.ParseName(attributeName);
            var newargument = SyntaxFactory.ParseAttributeArgumentList($"(\"{version}\")");
            return SyntaxFactory.Attribute(newname, newargument);
        }

        public static string GetAssemblyVersionFilePath(this Compilation compilation)
        {
            return compilation?.GetAssemblyVersionAttribute()?.ApplicationSyntaxReference.SyntaxTree.FilePath ?? string.Empty;
        }

        public static Version GetCurrentVersion(this Compilation compilation)
        {
            var attribute = compilation.GetAssemblyVersionAttribute();
            if (attribute == null)
            {
                return DefaultVersion();
            }

            try
            {
                var param = attribute.ConstructorArguments.First().Value.ToString();
                if (string.IsNullOrWhiteSpace(param))
                {
                    return DefaultVersion();
                }

                return new Version(param);
            }
            catch (Exception)
            {
                return DefaultVersion();
            }
        }

        public static string GetAssemblyName(this Compilation compilation) => compilation?.AssemblyName;

        public static string GetAssemblyFilePath(this Project project) => project?.FilePath;

        public static string GetTopoFilePath(this Topology topology) => $"{topology.AssemblyPath}.topo";

        public static bool TopoFileExists(string topofilepath)
        {
            var fi = new FileInfo(topofilepath);
            return fi.Exists && fi.Extension == ".topo";
        }

        public static Version GetNewAssemblyVersion(this Topology topology, SemanticChange change)
        {
            var version = topology.Version;

            switch (change)
            {
                case SemanticChange.Patch:
                    version = new Version(version.Major, version.Minor, version.Build + 1, 0);
                    break;
                case SemanticChange.Minor:
                    version = new Version(version.Major, version.Minor + 1, version.Build, 0);
                    break;
                case SemanticChange.Major:
                    version = new Version(version.Major + 1, version.Minor, version.Build, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(change));
            }

            return version;
        }

        public static void SetAssemblyVersion(this Compilation compilation, Version version)
        {
            var compilationunits = compilation.SyntaxTrees.SelectMany(t => t.GetRoot().DescendantNodesAndSelf())
                                              .OfType<CompilationUnitSyntax>()
                                              .Where(cu => cu.AttributeLists.Count > 0);

            var targets = compilationunits.SelectMany(t => t.DescendantNodesAndSelf())
                                          .OfType<AttributeSyntax>()
                                          .Where(at => at.Name.ToString().Equals("AssemblyVersion"))
                                          .ToList();

            if (!targets.Any())
            {
                throw new InvalidDataException($"{nameof(UpdateAssemblyVersion)} requires an AssemblyInfo.cs file");
            }

            foreach (var target in targets)
            {
                var temp = target.SyntaxTree.GetRoot().ReplaceNode(target, version.ToAttributeSyntax());
                var file = GetTemporaryAssemblyInfoFile();
                File.WriteAllText(file, temp.ToFullString());
                File.Copy(file, target.SyntaxTree.FilePath, true);
            }
        }

        /// <summary>
        /// Should update the assembly version in AssemblyInfo.cs as well as store a topo file of the new contract.
        /// </summary>
        public static void UpdateAssemblyVersion(this Compilation compilation, Topology topology, SemanticChange change, bool saveTopo = true)
        {
            if (compilation == null) throw new ArgumentNullException(nameof(compilation));

            if (saveTopo)
            {
                topology.SaveToFile();
            }

            var versionpath = compilation.GetAssemblyVersionFilePath();
            var newversion = topology.GetNewAssemblyVersion(change);

            compilation.SetAssemblyVersion(newversion);
        }

        private static string GetTemporaryAssemblyInfoFile()
        {
            var temp = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            Directory.CreateDirectory(temp);
            return Path.Combine(temp, "AssemblyInfo.cs");
        }

        public static List<(string, Version)> GetAssemblyReferences(this Compilation compilation)
        {
            if (compilation == null) throw new ArgumentNullException(nameof(compilation));
            return compilation.ReferencedAssemblyNames.Select(a => (a.Name, a.Version)).ToList();
        }

        public static List<string> GetAssemblyPublicApi(this Compilation compilation)
        {
            var publicMembers = new List<string>();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var types = syntaxTree.GetRoot().DescendantNodesAndSelf()
                                                .OfType<BaseTypeDeclarationSyntax>()
                                                .Where(td => td.Modifiers.Any(SyntaxKind.PublicKeyword));

                foreach (var type in types)
                {
                    var declaredSymbol = semanticModel.GetDeclaredSymbol(type);
                    var members = declaredSymbol.GetMembers().Where(m => m.DeclaredAccessibility == Accessibility.Public);

                    publicMembers.AddRange(members.Select(m => m.ToString()));
                }
            }

            return publicMembers;
        }
        
        public static Topology BuildTopology(Compilation compilation, Project project)
        {
            if (compilation == null) throw new ArgumentNullException(nameof(compilation));
            if (project == null) throw new ArgumentNullException(nameof(project));

            return new Topology(version: compilation.GetCurrentVersion(),
                                assemblyName: compilation.GetAssemblyName(),
                                assemblyPath: project.GetAssemblyFilePath(),
                                assemblyReferences: compilation.GetAssemblyReferences(),
                                publicApi: compilation.GetAssemblyPublicApi());
        }

        public static Topology LoadFromFile(string filepath) => JsonConvert.DeserializeObject<Topology>(File.ReadAllText(filepath));

        public static void SaveToFile(this Topology topology)
        {
            var path = topology.GetTopoFilePath();
            if (File.Exists(path)) { File.Delete(path); }
            File.WriteAllText(path, JsonConvert.SerializeObject(topology, Formatting.Indented));
        }

        public static bool Additions<T>(IEnumerable<T> newlist, IEnumerable<T> oldlist) where T : IComparable
        {
            if (newlist == null || oldlist == null) throw new ArgumentNullException();
            return newlist.Where(n => !oldlist.Contains(n)).Any();
        }

        public static bool Removals<T>(IEnumerable<T> newlist, IEnumerable<T> oldlist) where T : IComparable
        {
            if (newlist == null || oldlist == null) throw new ArgumentNullException();
            return oldlist.Where(o => !newlist.Contains(o)).Any();
        }

        public static SemanticChange DetermineSemanticChange(this Topology newTopology, Topology oldTopology)
        {
            var change = SemanticChange.Patch;

            if (Additions(newTopology.PublicApi, oldTopology.PublicApi))
            {
                change = SemanticChange.Minor;
            }

            if (Removals(newTopology.PublicApi, oldTopology.PublicApi))
            {
                change = SemanticChange.Major;
            }

            return change;
        }
    }
}
