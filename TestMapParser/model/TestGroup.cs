using System;
using System.Collections.Generic;

namespace TestMapParser
{
    public class TestGroup
    {
        public string ResourceGroup { get; set; }
        public List<CopyStep> CopySteps { get; set; } = new List<CopyStep>();
        public List<Execution> ExecutionSteps { get; } = new List<Execution>();
    }
}
