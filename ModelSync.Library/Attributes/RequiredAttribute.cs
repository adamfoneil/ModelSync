using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ModelSync.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter , AllowMultiple = false)]
    public class RequiredAttribute : Attribute 
    {
        public RequiredAttribute()
        {

        }
    }
}
