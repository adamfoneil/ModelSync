using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB6.net4_7
{
    [ComplexType]
    public class ContactInfo
    {
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
