using ModelSync.Library.Models;
using System.Collections.Generic;

namespace Testing.Models
{
    public class TestCase
    {
        public DataModel SourceModel { get; set; }
        public DataModel DestModel { get; set; }
        public List<ScriptAction> DiffActions { get; set; }
        public bool IsCorrect { get; set; }
        public string Comments { get; set; }
    }
}
