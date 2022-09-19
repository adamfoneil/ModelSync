using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; }
        public virtual EmployeeAddress EmployeeAddress { get; set; } //Navigation property Returns the Employee Address
        public virtual Department Department { get; set; }           //Navigation property Returns the Employee Department
        public virtual Grade Grade { get; set; }                     //Navigational Property for Grade
        public int GradeID { get; set; }                             //GradeID is defined here. EF will mark this as ForeignKey
        public virtual ICollection<Project> Projects { get; set; }   //Navigation property
    }
}
