using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketTracker.Models
{
    public class ApiTicket
    {
        public int TicketId { get; set; }
        public string AssignedUser { get; set; }
        public string AssignedGroup { get; set; }
        public List<Note> Notes { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Opened { get; set; }
        public DateTime? Closed { get; set; }
        public AssignmentStatus AssignmentStatus { get; set; }
        public int? ParentID { get; set; }
    }
}
