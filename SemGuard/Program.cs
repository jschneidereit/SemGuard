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
            if (!solution.HasValue())
            {
                Console.WriteLine("No solution specified");
                return 1;
            }
            
            var assembly = app.Option("-a |--assembly <assembly>", "The target assembly to examine", CommandOptionType.MultipleValue, false);
            if (!assembly.HasValue())
            {
                Console.WriteLine("No assembly name(s) specified");
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
                                
                var major = c.Option("--major", "Increment the major version.", CommandOptionType.NoValue);
                var minor = c.Option("--minor", "Increment the minor version.", CommandOptionType.NoValue);
                var patch = c.Option("--patch", "Increment the patch version.", CommandOptionType.NoValue);
                var build = c.Option("--build", "Increment the build version.", CommandOptionType.NoValue);

                c.OnExecute(() =>
                {
                    //Basically if an assemblyinfo.cs and/or nuspec contains any of these kinds of versions
                    //We will increment them. If they don't e.g.: "1.0.*" we'll just say something and exit?
                    if (major.HasValue())
                    {

                    }
                    else if (minor.HasValue())
                    {

                    }
                    else if (patch.HasValue())
                    {

                    }
                    else if (build.HasValue())
                    {
                        
                    }
                    else
                    {

                    }

                    return 0;
                });
            });
            
            app.Execute();

            //https://msdn.microsoft.com/en-us/magazine/mt763239.aspx

            return 0;
        }
}
}
