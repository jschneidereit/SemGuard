using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace SemGuard
{
    public class Orchestrator
    {
        private readonly List<(Topology, Compilation, Project)> _targets;
        
        public Orchestrator(string solution, List<string> assemblies)
        {
            var data = Transformer.LoadSolution(solution: solution);
            var targets = data.Where(t => assemblies.Contains(t.Item3.AssemblyName));
        }
    }
}
