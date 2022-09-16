using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Attributes
{

    [AttributeUsage(AttributeTargets.Property,AllowMultiple =false)]
    public class ForeignKeyAttribute : Attribute
    {
        public string Name { get; }
        public ForeignKeyAttribute(string name)
        {
            Name = name;
        }   
    }
}
