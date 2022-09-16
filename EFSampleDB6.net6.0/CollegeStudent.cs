using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB6.net6_0
{
    [Table("CollegeStudentMaster", Schema = "dbo")]   //To demonstrate some Data Annotations
    public class CollegeStudent
    {
        public int ID { get; set; }

        [Column("StudnetName")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar",Order =10)]
        public string Address1 { get; set; }

        [Column(TypeName = "varchar", Order = 20)]
        public string Address2 { get; set; }

        [Column(TypeName = "text", Order = 30)]
        public string Address3 { get; set; }

        [Column(TypeName = "money")]
        public decimal Donation { get; set; }

        [Column(TypeName = "decimal")]
        public decimal Height { get; set; }
        public double Weight { get; set; }
        public string Address { get; set; }
        [Required]   //we decorate the Name property with Required Attribute. This will create the Name column as Not Null in the database.
        public string Remarks { get; set; }
        [StringLength(maximumLength: 50, MinimumLength = 10)]
        public string Hobbies { get; set; } //Without the Required attribute, all string properties are mapped to NULLABLE columns 
        public DateTime DOB { get; set; }
        public int? Age { get; set; } //nullable field
        [NotMapped]
        public byte[] Photo { get; set; } //NotMapped Attribute on field
    }

}
