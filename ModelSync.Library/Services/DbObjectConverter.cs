using ModelSync.Library.Abstract;
using ModelSync.Library.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ModelSync.Library.Services
{
    public class DbObjectConverter : JsonConverter
    {
        private Dictionary<int, DbObject> _refs = new Dictionary<int, DbObject>();

        public override bool CanConvert(Type objectType)
        {
            return typeof(DbObject).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// a lot of help from https://stackoverflow.com/a/19308474/2023653
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Path.EndsWith("Parent") && reader.Value == null)
            {
                return null;
            }

            JObject jo = JObject.Load(reader);

            if (jo.ContainsKey("$ref"))
            {
                int refId = jo["$ref"].Value<int>();
                return _refs[refId];
            }

            ObjectType dbObjType = (ObjectType)((int)jo["ObjectType"]);

            DbObject dbObj;
            switch (dbObjType)
            {
                case ObjectType.Table:
                    dbObj = new Table();
                    break;

                case ObjectType.Column:
                    dbObj = new Column();
                    break;

                case ObjectType.ForeignKey:
                    dbObj = new ForeignKey();
                    break;

                case ObjectType.Index:
                    dbObj = new Index();
                    break;

                case ObjectType.Schema:
                    dbObj = new Schema();
                    break;

                default:
                    throw new Exception($"Unrecognized object type {dbObjType}");
            }          

            serializer.Populate(jo.CreateReader(), dbObj);

            _refs.Add(jo["$id"].Value<int>(), dbObj);
            return dbObj;
        }
        
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
