using System;
using System.Diagnostics;

namespace TestMapParser
{
    public class JestExecutor
    {
        public string Execute(Execution step)
        {
            var jestPath = step.Path;
            var args = step.Args;

            return $"{jestPath} {args}";
        }
    }
}
