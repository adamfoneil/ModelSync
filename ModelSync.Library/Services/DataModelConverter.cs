using ModelSync.Library.Models;
using Newtonsoft.Json;
using System;

namespace ModelSync.Library.Services
{
    public class DataModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(DataModel).IsAssignableFrom(objectType);
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
