using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace TicketTracker.Models
{
    public class EditUserViewModel
    {
        public AppUser User { get; set; }
        public string Id { get; set; }
        public string GroupName { get; set; }
        public string RoleName { get; set; }
        public IEnumerable<IdentityRole> Roles { get; set; }
    }
}
