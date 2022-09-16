using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Attributes
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple =false , Inherited = true)]
    public class KeyAttribute : Attribute
    {
        public KeyAttribute()
        {

        }
    }
}
