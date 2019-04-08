using System;
using System.IO;
using CommandLine;

namespace TestMapParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var totalAgents = 0;
            var currentAgent = 0;

            Parser.Default.ParseArguments<Options>(args)
                 .WithParsed<Options>(o =>
                 {
                     totalAgents = o.TotalAgents;
                     currentAgent = o.CurrentAgent;
                     WellKnownVars.Values[WellKnownVars.BuildRoot] = o.BuildRoot;
                     WellKnownVars.Values[WellKnownVars.WorkingDir] = o.WorkingDir;
                     WellKnownVars.Values[WellKnownVars.TestMapDir] = o.Source;
                 });

            var executingPath = System.AppDomain.CurrentDomain.BaseDirectory;
            // var totalAgents = Environment.GetEnvironmentVariable("SYSTEM_TOTALJOBSINPHASE");
            // var currentAgent = Environment.GetEnvironmentVariable("SYSTEM_JOBPOSITIONINPHASE");

            //var keys = Environment.GetEnvironmentVariables();

            Console.WriteLine("************ Booting up ****************");
            Console.WriteLine("Executing path: " + executingPath);
            Console.WriteLine("Total number of agents: " + totalAgents);
            Console.WriteLine("Currentt agent number: " + currentAgent);
            Console.WriteLine("****************************************");

            var testMap = new TestMapReader().ReadTestMap(Path.Join(WellKnownVars.Values[WellKnownVars.TestMapDir], "testmap.xml"), currentAgent);

            foreach (var tg in testMap.TestGroups)
            {
                var docFile = new DockerGenerator().Generate(tg);
                Console.WriteLine("Docker file generated: " + docFile);
            }
        }
    }

    public class Options
    {
        [Option('s', "source", Required = true, HelpText = "Testmap path")]
        public string Source { get; set; }
        [Option('t', "totalagents", Required = true, HelpText = "Total agents")]
        public int TotalAgents { get; set; }
        [Option('c', "currentagent", Required = true, HelpText = "Current agent")]
        public int CurrentAgent { get; set; }
        [Option('b', "buildroot", Required = false, HelpText = "Build binaries folder")]
        public string BuildRoot { get; set; } = string.Empty;
        [Option('w', "workingdir", Required = false, HelpText = "Container working dir")]
        public string WorkingDir { get; set; } = string.Empty;
    }
}
