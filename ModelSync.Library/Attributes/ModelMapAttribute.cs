using System;

namespace ModelSync.Library.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ModelMapAttribute : Attribute
    {
        public Type Type { get; set; }
        public ModelMapType ModelMapType { get; set; }
        public ModelMapAttribute(ModelMapType modelMapType, Type T)
        {
            ModelMapType = modelMapType;
            Type = T;
        }
        public ModelMapAttribute(ModelMapType modelMapType)
        {
            ModelMapType = modelMapType;
        }
    }
    public enum ModelMapType
    {
        Attribute,
        Config,
    }
}
