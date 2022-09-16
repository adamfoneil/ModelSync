using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB6.netstandard2_0
{
    public class Customer //To demonstrate ComplexType attribute
    {
        public Customer()
        {
            CustomerContact = new ContactInfo();
        }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public ContactInfo CustomerContact { get; set; } //ComplexType Attribute
    }
}
