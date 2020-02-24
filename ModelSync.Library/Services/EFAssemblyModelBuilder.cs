using ModelSync.Library.Interfaces;
using ModelSync.Library.Models;
using System;
using System.Reflection;

namespace ModelSync.Library.Services
{
    public class EFAssemblyModelBuilder : IAssemblyModelBuilder
    {
        public DataModel GetDataModel(Assembly assembly, string defaultSchema, string defaultIdentityColumn)
        {
            throw new NotImplementedException();
        }
    }
}
