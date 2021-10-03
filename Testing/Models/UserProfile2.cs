using AO.Models.Interfaces;
using AO.Models.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Testing.Models
{
    public class UserProfile2 : UserProfileBase, ITenantUser<int>, IUserBaseWithRoles, IModel<int>
    {
        public int? WorkspaceId { get; set; }

        public int TenantId => WorkspaceId ?? 0;

        [NotMapped]
        public new int Id { get => UserId; set => UserId = value; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        public HashSet<string> Roles { get; set; }

        public bool HasRole(string roleName) => Roles?.Contains(roleName) ?? false;

        public bool HasAnyRole(params string[] roleNames) => roleNames?.Any(role => HasRole(role)) ?? false;
    }
}
