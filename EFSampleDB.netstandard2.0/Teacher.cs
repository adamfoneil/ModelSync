using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB6.netstandard2_0
{

    [NotMapped]
    public class Teacher // NotMapped Attribute on tables
    {
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
        public string Title { get; set; }
    }
}
