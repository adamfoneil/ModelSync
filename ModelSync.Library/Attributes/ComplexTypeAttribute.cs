using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Attributes
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false)]
    public class ComplexTypeAttribute : Attribute 
    {
        public ComplexTypeAttribute()
        {

        }
    }
}
