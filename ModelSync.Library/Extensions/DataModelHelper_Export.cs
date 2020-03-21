using ModelSync.Library.Attributes;
using ModelSync.Library.Services;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ModelSync.Library.Extensions
{
    public static partial class DataModelHelper
    {
        public const string DataModelFileSuffix = ".DataModel.json";
        public const string DataModelErrorFileSuffix = ".DataModel.Error.json";

        /// <summary>
        /// Use this in your app's startup, bound to AppDomain.CurrentDomain.AssemblyLoad event to export data models
        /// from assemblies marked with the [ExportDataModel] attribute. This enables ModelSync to work around issues
        /// loading assemblies dynamically
        /// </summary>        
        public static void Export(object sender, AssemblyLoadEventArgs e)
        {
            var attr = e.LoadedAssembly.GetCustomAttribute<ExportDataModelAttribute>();
            if (attr != null)
            {
                ExportInner(e.LoadedAssembly, attr.DefaultSchema, attr.DefaultIdentityColumn);
            }
        }

        private static void ExportInner(Assembly assembly, string defaultSchema, string defaultIdentityColumn)
        {
            string buildOutputFile(string suffix)
            {
                return Path.Combine(Path.GetDirectoryName(assembly.Location), Path.GetFileNameWithoutExtension(assembly.Location) + suffix);
            }

            try
            {
                var dataModel = new AssemblyModelBuilder().GetDataModel(assembly, defaultSchema, defaultIdentityColumn);
                dataModel.SaveJson(buildOutputFile(DataModelFileSuffix));
            }
            catch (Exception exc)
            {
                try
                {
                    string errorJson = JsonConvert.SerializeObject(new { message = exc.Message });
                    File.WriteAllText(buildOutputFile(DataModelErrorFileSuffix), errorJson);
                }
                catch (Exception excInner)
                {
                    Debug.Print($"Export data model exception: {exc.Message}");
                    Debug.Print($"Couldn't write error log: {excInner.Message}");
                }
            }
        }
    }
}
