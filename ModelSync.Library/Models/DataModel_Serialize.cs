using ModelSync.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModelSync.Models
{
    public partial class DataModel
    {
        public static DataModel FromJsonFile(string fileName)
        {
            string json = File.ReadAllText(fileName);
            return FromJson(json);
        }

        public static DataModel FromJson(string json)
        {
            return JsonConvert.DeserializeObject<DataModel>(json, new DbObjectConverter());
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
        }

        public void SaveJson(string fileName)
        {
            string path = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllText(fileName, ToJson());
        }

        [JsonIgnore]
        public Dictionary<string, Table> TableDictionary
        {
            get { return Tables.ToDictionary(item => item.Name); }
        }
    }
}
