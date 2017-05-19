using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemGuard.Lib
{
    /// <summary>
    /// We are attempting to analyze a C# project and assign it a verion number based on http://semver.org/.
    /// This project currently has no concept of "no changes" to an assembly. 
    /// If you are running this tool against some source code, hopefully it's as part of a pull request.
    /// </summary>
    public enum SemanticChange
    {
        Patch = 0,
        Minor = 1,
        Major = 2
    }
}
