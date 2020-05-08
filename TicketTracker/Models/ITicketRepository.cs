using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketTracker.Models
{
    public interface ITicketRepository
    {
        IQueryable<Ticket> Tickets { get; }
        void SaveTicket(Ticket ticket);
        void CreateNote(string note, Ticket ticket);
        void UpdateChildren(Ticket parent);
        void UpdateChild(Ticket child, Ticket parent);
        void AddChild(Ticket child, Ticket parent);
        void RemoveChild(Ticket child);
        Ticket DeleteTicket(int ticketId);
    }
}
