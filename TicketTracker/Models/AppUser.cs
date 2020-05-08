using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TicketTracker.Models
{
    public enum AccessLevel
    {
        User,
        Admin
    }
    public class AppUser : IdentityUser
    {
        public AccessLevel AccessLevel { get; set; }
        public ICollection<UserGroup> AssigmentGroups { get; set; }
    }
}
