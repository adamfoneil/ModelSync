using ModelSync.Library.Attributes;
using ModelSync.Library.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace ModelSync.Library.Extensions
{
    public static class AssemblyHelper
    {        
        private static void ExportDataModel(Assembly assembly, string defaultSchema, string defaultIdentityColumn)
        {
            var dataModel = new AssemblyModelBuilder().GetDataModel(assembly, defaultSchema, defaultIdentityColumn);
            string json = JsonConvert.SerializeObject(dataModel, Formatting.Indented);
            string outputFile = Path.Combine(Path.GetDirectoryName(assembly.Location), Path.GetFileNameWithoutExtension(assembly.Location) + ".DataModel.json");
            File.WriteAllText(outputFile, json);
        }

        /// <summary>
        /// Use this in your app's startup, bound to AppDomain.CurrentDomain.AssemblyLoad event to export data models
        /// from assemblies marked with the [ExportDataModel] attribute. This enables ModelSync to work around issues
        /// loading assemblies dynamically
        /// </summary>        
        public static void ExportDataModel(object sender, AssemblyLoadEventArgs e)
        {
            var attr = e.LoadedAssembly.GetCustomAttribute<ExportDataModelAttribute>();
            if (attr != null)
            {
                ExportDataModel(e.LoadedAssembly, attr.DefaultSchema, attr.DefaultIdentityColumn);
            }
        }
    }
}
