using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TicketTracker.Models
{
    public class EFTicketRepository : ITicketRepository
    {
        AppIdentityDbContext context;
        public EFTicketRepository(AppIdentityDbContext ctx) => context = ctx;
        public IQueryable<Ticket> Tickets => context.Tickets
            .Include(t => t.AssignedUser)
            .Include(t => t.AssignedGroup)
            .Include(t => t.Notes)
            .Include(t => t.Children)
            .Include(t => t.Parent);
        public void CreateNote(string Note, Ticket ticket)
        {
            var tentry = context.Tickets.Include(t => t.Notes).FirstOrDefault(t => t.TicketId == ticket.TicketId);
            Note note = new Note
            {
                Description = Note,
                Added = DateTime.Now
            };
            var x = context.Note.Add(note);
            note.NoteId = x.Property(n => n.NoteId).CurrentValue;
            tentry.Notes.Add(note);
            context.SaveChanges();
        }
        public void SaveTicket(Ticket ticket)
        {
            if(ticket.TicketId == 0)
            {
                var content = context.Tickets.Add(ticket);
                ticket.TicketId = content.Property(t => t.TicketId).CurrentValue;
            }
            else if(ticket.Parent != null && !ticket.IsChild) // Initial save of a child
            {
                AddChild(ticket, ticket.Parent);
                UpdateChild(ticket, ticket.Parent);
            }
            else
            {
                if (ticket.AssignedUser != null) context.Attach(ticket.AssignedUser);
                Ticket dbentry = context.Tickets
                    .Include(t => t.AssignedUser)
                    .Include(t => t.AssignedGroup)
                    .Include(t => t.Parent)
                    .Include(t => t.Notes)
                    .Include(t => t.Children).FirstOrDefault(t => t.TicketId == ticket.TicketId);
                if(dbentry != null)
                {
                    dbentry.AssignedUser = ticket.AssignedUser;
                    dbentry.AssignedGroup = ticket.AssignedGroup;
                    dbentry.AssignmentStatus = ticket.AssignmentStatus;
                    dbentry.Closed = ticket.Closed;
                    dbentry.Description = ticket.Description;
                    dbentry.Parent = ticket.Parent;
                    dbentry.IsChild = ticket.IsChild;
                    dbentry.Title = ticket.Title;
                    dbentry.Opened = ticket.Opened;
                    context.Tickets.Update(dbentry);
                    //context.Entry(dbentry).State = EntityState.Detached;
                    if (dbentry.Children.Count > 0) UpdateChildren(dbentry);
                }
            }
            context.SaveChanges();
        }
        public void UpdateChildren(Ticket parent)
        {
            foreach(var child in parent.Children)
            {
                UpdateChild(child, parent);
            }
        }
        public void UpdateChild(Ticket child, Ticket parent)
        {
            child.IsChild = true;
            child.Description = parent.Description;
            child.AssignedGroup = parent.AssignedGroup;
            child.AssignedUser = parent.AssignedUser;
            child.AssignmentStatus = parent.AssignmentStatus;
            child.Closed = parent.Closed;
            child.Title = parent.Title;
            SaveTicket(child);
        }
        public void AddChild(Ticket child, Ticket parent)
        {
            var entry = Tickets.FirstOrDefault(t => t.TicketId == child.TicketId);
            entry.Parent = parent;
            context.Tickets.Update(entry);
            context.SaveChanges();
        }
        public void RemoveChild(Ticket child)
        {
            var entry = Tickets.FirstOrDefault(t => t.TicketId == child.TicketId);
            entry.Parent = null;
            entry.IsChild = false;
            child.IsChild = false;
            context.Tickets.Update(entry);
            context.SaveChanges();
        }
        public Ticket DeleteTicket(int id)
        {
            Ticket dbentry = context.Tickets.FirstOrDefault(t => t.TicketId == id);
            if(dbentry != null)
            {
                context.Tickets.Remove(dbentry);
                context.SaveChanges();
            }
            return dbentry;
        }
    }
}
