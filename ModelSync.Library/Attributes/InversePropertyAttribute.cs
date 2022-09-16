using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Attributes
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple =false)]
    public class InversePropertyAttribute : Attribute 
    {
        public string Property { get; }    
        public InversePropertyAttribute(string property)
        {
            Property = property;
        }
    }
}
