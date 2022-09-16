using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Attributes
{
    [AttributeUsage(AttributeTargets.Property| AttributeTargets.Parameter, AllowMultiple = false )]
    public class StringLengthAttribute : Attribute 
    {
        public int MaximumLength { get; }
        public int MinimumLength { get; set; }
        public StringLengthAttribute(int maximumLength)
        {
            MaximumLength = maximumLength;  
        }
    }
}
