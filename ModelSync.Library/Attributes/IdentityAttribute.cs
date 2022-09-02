using System;

namespace ModelSync.Library.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IdentityAttribute : Attribute
    {
        public IdentityAttribute(string propertyName, int seed, int increment)
        {
            PropertyName = propertyName;
            Seed = seed;
            Increment = increment;
        }

        public string PropertyName { get; }
        public int Seed { get; }
        public int Increment { get; }
    }
}
