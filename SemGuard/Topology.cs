using System;
using System.Collections.Generic;

namespace SemGuard
{
    /// <summary>
    /// Immutable plain-ol'-data that contains information about an assembly
    /// </summary>
    public struct Topology
    {
        /// <summary>
        /// The version of the target assembly
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The name of the target assembly
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// 
        /// </summary>
        public string AssemblyPath { get; }

        /// <summary>
        /// The names and versions of each referenced assembly
        /// </summary>
        public List<(string, Version)> AssemblyReferences { get; }

        /// <summary>
        /// The public facing API of the target assembly
        /// </summary>
        public List<string> PublicApi { get; }

        /// <summary>
        /// The constructor for this immutable plain-ol'-data container, you cannot create an empty one and assign values later on.
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        public Topology(Version version, string assemblyName, string assemblyPath, List<(string, Version)> assemblyReferences, List<string> publicApi)
        {
            //TODO: figure out how to deal with relative paths, or remove assembly path altogether
            Version = version ?? throw new ArgumentNullException(nameof(version));
            AssemblyName = string.IsNullOrWhiteSpace(assemblyName) ? throw new ArgumentNullException(nameof(assemblyName)) : assemblyName;
            AssemblyPath = string.IsNullOrWhiteSpace(assemblyPath) ? throw new ArgumentNullException(nameof(assemblyPath)) : assemblyPath;
            AssemblyReferences = assemblyReferences ?? throw new ArgumentNullException(nameof(AssemblyReferences));
            PublicApi = publicApi ?? throw new ArgumentNullException(nameof(publicApi));
        }
    }
}
