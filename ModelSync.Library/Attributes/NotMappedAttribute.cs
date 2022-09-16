using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property,AllowMultiple =false)]
    public class NotMappedAttribute : Attribute 
    {
    }
}
