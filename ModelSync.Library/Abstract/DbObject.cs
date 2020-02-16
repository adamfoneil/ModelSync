using ModelSync.Library.Models;
using System;
using System.Collections.Generic;

namespace ModelSync.Library.Abstract
{
    public enum ObjectType
    {
        Schema,
        Table,
        Column,
        Index,
        ForeignKey
    }

    public abstract class DbObject
    {        
        public string Name { get; set; }
        public DbObject Parent { get; set; }
        
        public abstract ObjectType ObjectType { get; }
        public abstract string CreateStatement();
        public abstract string DropStatement();
        public abstract IEnumerable<DbObject> GetDropDependencies(DataModel dataModel);
        public abstract bool IsAltered(DbObject @object);

        public const string DefaultSchema = "dbo";

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            var dbObj = obj as DbObject;
            return (dbObj != null) ? dbObj.Name?.ToLower().Equals(Name?.ToLower()) ?? false : false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public string Schema
        {
            get
            {
                try
                {
                    var parts = Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    return (parts.Length > 1) ? parts[0] : DefaultSchema;
                }
                catch 
                {
                    return null;
                }                
            }
        }
    }
}
