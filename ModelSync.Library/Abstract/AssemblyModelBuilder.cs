using ModelSync.Library.Extensions;
using ModelSync.Library.Interfaces;
using ModelSync.Library.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Library.Abstract
{
    /// <summary>
    /// describes methods for building a DataModel from an Assembly
    /// </summary>
    public abstract class AssemblyModelBuilder : IModelBuilder
    {
        private readonly Assembly _assembly;

        public AssemblyModelBuilder(Assembly assembly)
        {
            _assembly = assembly;
        }

        public abstract Dictionary<Type, string> DataTypes { get; }

        /// <summary>
        /// override this to look for certain attributes or execute other logic to build out a Column 
        /// object in ways that aren't determined by Type alone        
        /// </summary>
        protected virtual void SetColumnProperties(PropertyInfo propertyInfo, Column column)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }
            // do nothing by default
        }


        public Task<DataModel> GetDataModelAsync()
        {
            throw new NotImplementedException();
        }
    }
}
