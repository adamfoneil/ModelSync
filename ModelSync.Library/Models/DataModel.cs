using ModelSync.Library.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModelSync.Library.Models
{
    public partial class DataModel
    {
        public DataModel()
        {
            Schemas = Enumerable.Empty<Schema>();
            Tables = Enumerable.Empty<Table>();
            ForeignKeys = Enumerable.Empty<ForeignKey>();
        }

        public IEnumerable<Schema> Schemas { get; set; }
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }

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
            File.WriteAllText(fileName, ToJson());
        }

        [JsonIgnore]
        public Dictionary<string, Table> TableDictionary
        {
            get { return Tables.ToDictionary(item => item.Name); }
        }
    }
}
