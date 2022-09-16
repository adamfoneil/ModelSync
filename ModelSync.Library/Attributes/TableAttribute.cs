using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Attributes
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false)]
    public class TableAttribute : Attribute 
    {
        public string Name { get; }
        public string Schema { get; set; }
        public TableAttribute(string name)
        {
            Name = name;
        }
    }
}
