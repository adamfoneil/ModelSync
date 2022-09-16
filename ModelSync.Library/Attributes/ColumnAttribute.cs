using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Attributes
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false )]
    public class ColumnAttribute : Attribute 
    {
        public string Name { get; }
        public int Order { get; set; } = -1;
        public string TypeName { get; set; }
        public ColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
