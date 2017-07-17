using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using SemGuard.Lib;

namespace SemGuard
{
    internal class Program
    {
        internal static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "sembump",
                Description = "Executes semantic versioning api analysis based on roslyn C# solutions",
            };

            var solution = app.Option("-s |--solution <solution>", "The path of the solution to be loaded", CommandOptionType.SingleValue, false);
            var assembly = app.Option("-a |--assembly <assembly>", "The target assembly to examine", CommandOptionType.MultipleValue, false);
            if (!solution.HasValue() || !assembly.HasValue())
            {
                Console.WriteLine("No solution specified");
                return 1;
            }

            Orchestrator orchestrator;
            try
            {
                orchestrator = new Orchestrator(solution: solution.Value(), assemblies: assembly.Values);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }

            app.Command("diff", c =>
            {
                c.Description = "Compares previous version's metadata against the current compilation and suggests a new version.";

                c.OnExecute(() =>
                {
                    return 0;
                });
            });

            app.Command("init", c =>
            {
                c.Description = "Generates the current version's metadata file based on the existing public facing api.";

                c.OnExecute(() =>
                {
                    return 0;
                });
            });

            app.Command("bump", c =>
            {
                c.Description = "Compares previous version's metadata against the current compilation and sets a new version.";

                c.OnExecute(() =>
                {
                    return 0;
                });
            });



            app.Execute();

            //https://msdn.microsoft.com/en-us/magazine/mt763239.aspx

            return 0;
        }
}
}
