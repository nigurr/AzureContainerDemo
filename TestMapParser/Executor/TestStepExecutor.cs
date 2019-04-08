using System;
using System.Xml;
using System.Xml.Linq;

namespace TestMapParser
{
    public class TestStepExecutor
    {
        public string ExecuteStep(Execution step)
        {
            switch (step.Type)
            {
                case "VsTest":
                    return new VsTestExecutor().Execute(step);
                case "Jest":
                    return new JestExecutor().Execute(step);
            }

            return null;
        }
    }
}
