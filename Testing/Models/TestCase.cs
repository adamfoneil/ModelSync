using System.Collections.Generic;

namespace Testing.Models
{
    /// <summary>
    /// copied from "C:\Users\Adam\Source\Repos\ModelSync.WinForms\ModelSync.App\Models\TestCase.cs"
    /// do not modify here unless you coordinate change there
    /// </summary>
    public class TestCase
    {
        public List<string> SqlCommands { get; set; }
        public bool IsCorrect { get; set; }
        public string Comments { get; set; }
    }
}
