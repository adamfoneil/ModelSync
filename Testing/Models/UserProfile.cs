using AO.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testing.Models
{
    [Table("AspNetUsers")]
    [Identity(nameof(UserId))]
    public class UserProfile
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}
