using System;
using System.Diagnostics;

namespace TestMapParser
{
    public class VsTestExecutor
    {
        public string Execute(Execution step)
        {
            var vstestConsolePath = Environment.GetEnvironmentVariable("vstestconsole.path");
            var source = step.Path;
            var args = step.Args;

            return $"dotnet vstest {source} {args} --logger:trx --ResultsDirectory:/share";
        }
    }
}
