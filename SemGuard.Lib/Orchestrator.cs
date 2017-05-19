using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace SemGuard.Lib
{
    public class Orchestrator
    {
        private readonly (Topology, Compilation, Project) _target;

        public Orchestrator(string solution, string assembly)
        {
            var data = Transformer.LoadSolution(solution: solution);
            _target = data.First(t => t.Item3.AssemblyName == assembly);
        }
    }
}
