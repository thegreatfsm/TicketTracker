using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketTracker.Models
{
    public class UserGroup
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int GroupId { get; set; }
        public AssignmentGroup Group { get; set; }
    }
}
