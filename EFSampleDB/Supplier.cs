using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB
{
    public class Supplier  //To demonstrate ComplexType attribute
    {
        public Supplier()
        {
            SupplierContact = new ContactInfo();
        }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public ContactInfo SupplierContact { get; set; } //ComplexType Attribute
    }
}
