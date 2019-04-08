using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace TestMapParser
{
    public class TestMapReader
    {
        public TestMap ReadTestMap(string path, int testGroupId)
        {
            var testMap = new TestMap();
            var xreader = new XmlDocument();

            xreader.Load(path);

            var testGroupList = xreader.SelectNodes("/TestMap/TestJobGroup");
            if (testGroupId >= testGroupList.Count)
            {
                return testMap;
            }

            var testGroups = testGroupList.Cast<XmlNode>().Where((x, i) => i % testGroupId == 0);

            foreach (XmlNode tg in testGroups)
            {
                var testGroupPath = tg.Attributes["ConfigPath"].Value;
                var testGroup = new TestGroup();

                xreader.Load(Path.Combine(WellKnownVars.Values[WellKnownVars.TestMapDir], testGroupPath));

                var resource = xreader.SelectSingleNode("/TestJobGroup/ResourceSpec/Resource");
                testGroup.ResourceGroup = resource.Attributes["Image"].Value;

                var copyStepsList = xreader.SelectNodes("/TestJobGroup/Setup/BuildFiles/Copy");
                foreach (XmlNode copyStep in copyStepsList)
                {
                    var c = new CopyStep(){
                        Source = copyStep.Attributes["Src"].Value,
                        Destination = copyStep.Attributes["Dest"].Value
                    };
                    testGroup.CopySteps.Add(c);
                }

                var executionList = xreader.SelectNodes("/TestJobGroup/TestJob/Execution");
                foreach (XmlNode execution in executionList)
                {
                    var e = new Execution()
                    {
                        Type = execution.Attributes["Type"].Value,
                        Args = execution.Attributes["Args"].Value,
                        Path = execution.Attributes["Path"].Value
                    };
                    testGroup.ExecutionSteps.Add(e);
                }

                testMap.TestGroups.Add(testGroup);
            }

            return testMap;
        }
    }
}
