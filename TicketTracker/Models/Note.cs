using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketTracker.Models
{
    public class Note
    {
        public int NoteId { get; set; }
        public DateTime Added { get; set; }
        public string Description { get; set; }

    }
}
