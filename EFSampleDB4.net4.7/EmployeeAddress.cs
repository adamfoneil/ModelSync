using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB6.net4_7
{
    public class EmployeeAddress  //To demonstrate One-to-One Relationship
    {
        [Key, ForeignKey("Employee")] //Defining the ForeignKey 
        public int EmployeeID { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public virtual Employee Employee { get; set; } //Navigation property Returns the Employee object
    }
}
