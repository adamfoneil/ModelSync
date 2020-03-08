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
                var sourceModel = Extract(resourceName, "SourceModel.json", (content) => DataModel.FromJson(content));
                var destModel = Extract(resourceName, "DestModel.json", (content) => DataModel.FromJson(content));
                var testCase = Extract<TestCase>(resourceName, "TestCase.json");
                
                var diff = DataModel.Compare(sourceModel, destModel);
                var commands = diff.SelectMany(scr => scr.Commands);
                Assert.IsTrue(commands.SequenceEqual(testCase.SqlCommands));
            }
        }

        private T Extract<T>(string resourceName, string entryName, Func<string, T> objectBuilder = null) where T : class
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var entry = zip.GetEntry(entryName);
                    using (var entryStream = entry.Open())
                    {
                        using (var reader = new StreamReader(entryStream))
                        {
                            string json = reader.ReadToEnd();
                            return objectBuilder?.Invoke(json) ?? JsonConvert.DeserializeObject<T>(json);
                        }
                    }
                }
            }
        }
    }
}
