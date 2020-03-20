using ModelSync.Library.Services;
using Newtonsoft.Json;
using System.IO;

namespace ModelSync.Library.Models
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
            File.WriteAllText(fileName, ToJson());
        }

    }
}
