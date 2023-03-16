﻿using AO.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testing.Models
{
    [Table("AspNetUsers-simpler")]
    [Identity(nameof(UserId))]
    public class UserProfile
    {
        [Key]
        [MaxLength(450)]
        public string Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}
