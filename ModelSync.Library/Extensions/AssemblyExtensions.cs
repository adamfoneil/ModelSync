using ModelSync.Library.Services;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace ModelSync.Library.Extensions
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Use this in your app's startup to export a data model to json at your assembly's location.
        /// This is handy when your assembly won't load via reflection
        /// </summary>        
        public static void ExportDataModel(this Assembly assembly, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var dataModel = new AssemblyModelBuilder().GetDataModel(assembly, defaultSchema, defaultIdentityColumn);
            string json = JsonConvert.SerializeObject(dataModel, Formatting.Indented);
            string outputFile = Path.Combine(Path.GetDirectoryName(assembly.Location), Path.GetFileNameWithoutExtension(assembly.Location) + ".DataModel.json");
            File.WriteAllText(outputFile, json);
        }
    }
}
