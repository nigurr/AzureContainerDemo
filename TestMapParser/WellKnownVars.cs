using System;
using System.Collections.Generic;

namespace TestMapParser
{
    public class WellKnownVars
    {
        public static Dictionary<string, string> Values = new Dictionary<string, string>(){
            {BuildRoot, string.Empty},
            {WorkingDir, string.Empty},
            {TestMapDir, string.Empty}
        };

        public const string BuildRoot = "[BuildRoot]";
        public const string WorkingDir = "[WorkingDirectory]";
        public const string TestMapDir = "";
    }
}