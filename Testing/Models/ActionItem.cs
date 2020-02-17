using AO.DbSchema.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Testing.Models
{
    public class ActionItem
    {
        public int Id { get; set; }

        [References(typeof(Employee), CascadeDelete = true)]
        public int EmployeeId { get; set; }

        [MaxLength(255)]
        [Required]
        public string Description { get; set; }

        public bool IsComplete { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
