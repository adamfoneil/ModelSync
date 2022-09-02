using ModelSync.Interfaces;
using ModelSync.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModelSync.Library.Services
{
    public class EFModelBuilder : IAssemblySource
    {
        public DataModel GetDataModel(Assembly assembly, string defaultSchema, string defaultIdentityColumn)
        {
            throw new NotImplementedException();
        }

        public DataModel GetDataModel(IEnumerable<Type> types, string defaultSchema, string defaultIdentityColumn)
        {
            throw new NotImplementedException();
        }
    }
}
