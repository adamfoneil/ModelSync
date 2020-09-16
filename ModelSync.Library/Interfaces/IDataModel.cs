using ModelSync.Abstract;
using ModelSync.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Interfaces
{
    public interface IDataModel
    {
        Dictionary<Type, string> Errors { get; set; }
        IEnumerable<ForeignKey> ForeignKeys { get; set; }
        IEnumerable<Schema> Schemas { get; set; }        
        IEnumerable<Table> Tables { get; set; }

        Task<IEnumerable<ScriptAction>> CreateIfNotExistsAsync(IDbConnection connection, SqlDialect dialect);
        void SaveJson(string fileName);
        string ToJson();
    }
}