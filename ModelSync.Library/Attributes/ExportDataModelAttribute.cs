using System;

namespace ModelSync.Attributes
{
    /// <summary>
    /// indicates that you'd like to export this assembly's data model to json during app startup
    /// to help ModelSync when an assembly can't be loaded dynamically
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class ExportDataModelAttribute : Attribute
    {
        public ExportDataModelAttribute(string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            DefaultSchema = defaultSchema;
            DefaultIdentityColumn = defaultIdentityColumn;
        }

        public string DefaultSchema { get; }
        public string DefaultIdentityColumn { get; }
    }
}
