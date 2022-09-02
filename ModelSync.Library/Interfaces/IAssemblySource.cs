using ModelSync.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModelSync.Interfaces
{
    public interface IAssemblySource
    {
        DataModel GetDataModel(Assembly assembly, string defaultSchema, string defaultIdentityColumn);
        DataModel GetDataModel(IEnumerable<Type> types, string defaultSchema, string defaultIdentityColumn);
    }
}
