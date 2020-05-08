using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketTracker.Models
{
    public class AssignmentGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserGroup> Users { get; set; }
    }
}
