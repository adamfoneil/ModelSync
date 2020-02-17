using AO.DbSchema.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Testing.Models
{
    [Identity(nameof(Id))]
    public class Employee
    {        
        public int Id { get; set; }
     
        [MaxLength(30)]
        [Required]
        [Key]
        public string UniqueId { get; set; }

        [MaxLength(50)]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(50)]
        [Required]
        public string LastName { get; set; }

        public DateTime? HireDate { get; set; }

        public DateTime? TermDate { get; set; }
    }
}
