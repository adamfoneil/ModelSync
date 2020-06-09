using ModelSync.Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

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
        public int ObjectId { get; set; }

        public abstract ObjectType ObjectType { get; }
        public abstract IEnumerable<string> CreateStatements();
        public abstract string DropStatement();
        public abstract IEnumerable<DbObject> GetDropDependencies(DataModel dataModel);
        public abstract bool IsAltered(DbObject @object, out string comment);
        public abstract Task<bool> ExistsAsync(IDbConnection connection, SqlDialect dialect);

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            var dbObj = obj as DbObject;
            return (dbObj != null) ? NamesAreEqual(this, dbObj) && ParentsAreEqual(this, dbObj) : false;
        }

        private static bool ParentsAreEqual(DbObject object1, DbObject object2)
        {
            if (object1.Parent == null ^ object2.Parent == null) return false;
            if (object1.Parent == null && object2.Parent == null) return true;

            try
            {
                return object1.Parent.Equals(object2.Parent);
            }
            catch
            {
                return false;
            }
        }

        private static bool NamesAreEqual(DbObject object1, DbObject object2)
        {
            try
            {
                return object1.Name.ToLower().Equals(object2.Name.ToLower());
            }
            catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (Parent?.Name?.ToLower().GetHashCode() ?? 0) + Name?.ToLower().GetHashCode() ?? 0;
        }

        public string GetSchema(string defaultSchema)
        {
            try
            {
                var parts = Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                return (parts.Length > 1) ? parts[0] : defaultSchema;
            }
            catch
            {
                return defaultSchema;
            }
        }

        public string GetBaseName()
        {
            try
            {
                var parts = Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                return (parts.Length > 1) ? parts[1] : Name;
            }
            catch
            {
                return Name;
            }
        }

        public IEnumerable<string> DropStatements(DataModel dataModel)
        {
            foreach (var obj in GetDropDependencies(dataModel))
            {
                yield return obj.DropStatement();
            }

            yield return DropStatement();
        }

        public IEnumerable<string> RebuildStatements(DataModel dataModel, string comment)
        {
            yield return $"-- {comment}";

            var deps = GetDropDependencies(dataModel);

            foreach (var dep in deps) yield return dep.DropStatement();

            yield return DropStatement();

            foreach (var c in CreateStatements()) yield return c;

            foreach (var dep in deps) yield return dep.CreateStatement();
        }

        public string CreateStatement() => string.Join("\r\n", CreateStatements());
    }
}
