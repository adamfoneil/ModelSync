using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB6.netstandard2_0
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
