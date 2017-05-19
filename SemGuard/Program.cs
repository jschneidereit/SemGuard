using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using SemGuard.Lib;

namespace SemGuard
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            CommandArgument assembly = null;
            CommandArgument solution = null;

            app.Command("assembly", (target) => assembly = target.Argument("assembly", "Enter the name of the assembly to be modified", false));
            app.Command("solution", (target) => solution = target.Argument("solution", "Enter the path of the solution to be loaded", false));

            //option show diff for an assembly

            Func<int> func = () =>
            {
                if (!File.Exists(solution.Value))
                {
                    Console.WriteLine("You must provide a valid solution");
                    return -1;
                }

                var orchestrator = new Orchestrator(solution.Value, assembly.Value);

                return 0;
            };

            app.OnExecute(func);

            //https://msdn.microsoft.com/en-us/magazine/mt763239.aspx
        }
    }
}
