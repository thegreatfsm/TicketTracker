using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TicketTracker.Models
{
    public enum AssignmentStatus
    {
        Open,
        InProgress,
        Resolved
    }
    public class Ticket
    {
        public int TicketId { get; set; }
        public AppUser AssignedUser { get; set; }
        public AssignmentGroup AssignedGroup { get; set; }
        public List<Note> Notes { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime Opened { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yy}", NullDisplayText = "In Progess")]
        public DateTime? Closed { get; set; }
        public AssignmentStatus AssignmentStatus { get; set; }
        public Ticket Parent { get; set; }
        public bool IsChild { get; set; }
        public List<Ticket> Children { get; set; }

    }
}
