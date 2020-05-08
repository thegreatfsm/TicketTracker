using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketTracker.Models
{
    public class ListViewModel
    {
        public IEnumerable<Ticket> Tickets { get; set; }
        public int? TicketID { get; set; }
        public AssignmentStatus? AssignmentStatus { get; set; }
        public string Title { get; set; }
        public string User { get; set; }
        public string[] Groups { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }
}
