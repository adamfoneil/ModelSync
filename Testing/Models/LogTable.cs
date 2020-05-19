using AO.Models;
using System;

namespace Testing.Models
{
    [Schema("log")]
    public class LogTable
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
    }
}
