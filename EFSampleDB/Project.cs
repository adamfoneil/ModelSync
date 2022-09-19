using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB
{

    public class Project  //To demonstrate Many-to-Many Relaionship. a join table ProjectEmployees will be created.
    {
        public int ProjectID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Employee> Employees { get; set; } //Navigation property
    }

}
