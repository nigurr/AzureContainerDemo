using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TestMapParser
{
    class DockerGenerator
    {
        public string Generate(TestGroup testGroup)
        {
            var sb = new StringBuilder();

            sb.AppendLine("#### Image ####");
            sb.AppendLine($"FROM {testGroup.ResourceGroup}");
            sb.AppendLine();

            sb.AppendLine("#### Copy files ####");
            foreach (var copy in testGroup.CopySteps)
            {
                sb.AppendLine($"RUN mkdir -p {copy.Destination.Replace("\\", "/").Replace(WellKnownVars.WorkingDir, WellKnownVars.Values[WellKnownVars.WorkingDir])}");
                sb.AppendLine($"COPY {copy.Source.Replace("\\", "/").Replace(WellKnownVars.BuildRoot, WellKnownVars.Values[WellKnownVars.BuildRoot])} {copy.Destination.Replace("\\", "/").Replace(WellKnownVars.WorkingDir, WellKnownVars.Values[WellKnownVars.WorkingDir])}");
            }
            sb.AppendLine();

            sb.AppendLine("#### Setup Steps ####");
            sb.AppendLine();

            var script = GenerateScript(testGroup.ExecutionSteps);
            var scriptName = Path.GetFileName(script);
            sb.AppendLine("#### Entry Point ####");
            sb.AppendLine($"COPY {scriptName} /");
            sb.AppendLine($"RUN [\"chmod\", \"+x\", \"/{scriptName}\"]");
            sb.AppendLine($"ENTRYPOINT /{scriptName}");

            var docFile = GetNextFileName(Path.Combine(WellKnownVars.Values[WellKnownVars.TestMapDir], "Dockerfile"));
            File.WriteAllText(docFile, sb.ToString());
            return docFile;
        }

        private string GenerateScript(List<Execution> executionSteps)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#!/bin/bash");

            foreach (var executionStep in executionSteps)
            {
                var script = new TestStepExecutor().ExecuteStep(executionStep);
                sb.AppendLine($"{script.Replace("\\", "/").Replace(WellKnownVars.WorkingDir, WellKnownVars.Values[WellKnownVars.WorkingDir])}");
            }

            var fileName = GetNextFileName(Path.Combine(WellKnownVars.Values[WellKnownVars.TestMapDir], "runtests.sh"));
            File.WriteAllText(fileName, sb.ToString());
            return fileName;
        }

        private string GetNextFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            string pathName = Path.GetDirectoryName(fileName);
            string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(fileName));
            int i = 0;
            // If the file exists, keep trying until it doesn't
            while (File.Exists(fileName))
            {
                i += 1;
                fileName = string.Format("{0}{1}{2}", fileNameOnly, i, extension);
            }
            return fileName;
        }
    }
}