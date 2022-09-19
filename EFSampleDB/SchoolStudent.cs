using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB
{

    public class SchoolStudent  //To demonstrate defalut convention
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public decimal Donation { get; set; }
        public decimal Height { get; set; }
        public double Weight { get; set; }
        public string Address { get; set; }
        public string Remarks { get; set; }
        public string Hobbies { get; set; }
        public DateTime DOB { get; set; }
        public int? Age { get; set; }
        public byte[] Photo { get; set; }
        public BloodGroup BloodGroup { get; set; } //Enum support
    }
    public enum BloodGroup
    {
        A_Positive,
        B_Positive,
        AB_Positive,
        O_Positive,
        A_Negative,
        B_Negative,
        AB_Negative,
        O_Negative
    }

}
