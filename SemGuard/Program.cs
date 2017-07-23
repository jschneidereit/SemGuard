using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using SemGuard.Lib;
using b = SemBump.Bumper;

namespace SemGuard
{
    internal class Program
    {
        internal static void SimpleFileValidation(CommandOption option, string name)
        {
            if (option.HasValue() && File.Exists(option.Value()))
            {
                return;
            }
            
            throw new InvalidProgramException($"Option {name} is invalid. It is necessary for this operation.");            
        }

        internal static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "sembump",
                Description = "Executes semantic versioning api analysis based on roslyn C# solutions.",
            };

            var solution = app.Option("-s |--solution <solution>", "The path of the solution to be loaded.", CommandOptionType.SingleValue, false);
            var assembly = app.Option("-a |--assembly <assembly>", "The target assembly to examine.", CommandOptionType.SingleValue, false);
          

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
                    SimpleFileValidation(solution, nameof(solution));
                    SimpleFileValidation(assembly, nameof(assembly));

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

                var nuspec = c.Option("--nuspec", 
$@"Optional target nuspec file (can be chocolatey or nuget nuspec file). {Environment.NewLine}
This currently is in 'bump' because there's no reason to analyze a nuspec file (in diff) that I can think of.{Environment.NewLine}
In fact, i'd rather not use a nuspec file for .NET projects, only for choco packages.{ Environment.NewLine}
You can get everything you need from 'nuget pack myassembly.csproj'", 
                    CommandOptionType.SingleValue);
                
                c.OnExecute(() =>
                {
                    FileInfo fi = null;
                    if (nuspec.HasValue())
                    {
                        fi = new FileInfo(nuspec.Value());
                    }

                    var operation = string.Empty;
                                        
                    if (major.HasValue())
                    {
                        operation = "major";
                    }
                    else if (minor.HasValue())
                    {
                        operation = "minor";
                    }
                    else if (patch.HasValue())
                    {
                        operation = "patch";
                    }
                    else if (build.HasValue())
                    {
                        operation = "build";
                    }
                    else
                    {
                        throw new NotImplementedException(
$@"Currently I don't believe there's a value to bumping a nuspec file if you have an assembly.{Environment.NewLine}
If you have an assembly and you want the nuspec version to stay in-line with it, it is likely better to just point nuget at your csproj.{Environment.NewLine}
Eventually, if a case can be made for it, I'd like to see this use 'diff' to determine what kind of bump is required and execute it.");
                    }

                    if (nuspec.HasValue())
                    {
                        fi = new FileInfo(nuspec.Value());
                        if (!fi.Exists)
                        {
                            Console.WriteLine("Nuspec file does not exist, exiting.");
                            return 1;
                        }

                        try
                        {
                            b.BumpNuspecFile(fi, operation);
                        }
                        catch (FormatException fe)
                        {
                            throw new InvalidDataException($"Caught a format exception. Message: {fe.Message}{Environment.NewLine}Likely an invalid version is in your nuspec.");
                        }
                    }

                    return 0;
                });
            });

            try
            {
                app.Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }

            //https://msdn.microsoft.com/en-us/magazine/mt763239.aspx

            return 0;
        }
}
}
