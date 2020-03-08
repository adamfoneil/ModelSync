using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Models;
using ModelSync.Library.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Testing.Models;

namespace Testing
{
    [TestClass]
    public class EmbeddedCases
    {
        [TestMethod]
        public void TestAllSamples()
        {
            var embedded = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(name => Path.GetExtension(name).Equals(".zip"));

            foreach (var resourceName in embedded)
            {
                TestCase testCase = ExtractTestCase(resourceName);

                //testCase.SourceModel.SaveJson(@"c:\users\adam\desktop\sourceModel.json");

                var diff = DataModel.Compare(testCase.SourceModel, testCase.DestModel);
                var commands = diff.SelectMany(scr => scr.Commands);
                Assert.IsTrue(commands.SequenceEqual(testCase.SqlCommands));
            }
        }

        private TestCase ExtractTestCase(string resourceName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var entry = zip.Entries.First();
                    using (var entryStream = entry.Open())
                    {
                        using (var reader = new StreamReader(entryStream))
                        {
                            string json = reader.ReadToEnd();
                            return JsonConvert.DeserializeObject<TestCase>(json, new DbObjectConverter());
                        }                        
                    }
                }
            }
        }
    }
}
