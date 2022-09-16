using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB6.netstandard2_0
{
    public class Grade
    {
        public int GradeID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Employee> Employees { get; set; } //Navigation property
    }
}
