using ModelSync.Library.Models;
using System.Collections.Generic;

namespace ModelSync.Library.Abstract
{
    public enum ObjectType
    {
        Table,
        Column,
        Index,
        ForeignKey
    }

    public abstract class DbObject
    {        
        public string Name { get; set; }
        public DbObject Parent { get; set; }

        public abstract bool HasSchema { get; }
        public abstract ObjectType ObjectType { get; }
        public abstract string CreateStatement();
        public abstract string DropStatement();
        public abstract IEnumerable<DbObject> GetDropDependencies(DataModel dataModel);
    }
}
