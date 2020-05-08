using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TicketTracker.Models;
using Microsoft.AspNetCore.JsonPatch;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TicketTracker.Controllers
{
    [Route("api/ticket")]
    public class TicketApiController : Controller
    {
        private ITicketRepository ticketRepository;
        private IGroupRepository groupRepository;
        private UserManager<AppUser> userManager;
        public TicketApiController(ITicketRepository trepo, IGroupRepository grepo, UserManager<AppUser> usrManager)
        {
            ticketRepository = trepo;
            groupRepository = grepo;
            userManager = usrManager;
        }
        [HttpGet]
        public IEnumerable<ApiTicket> Get() {
            var apiTickets = new List<ApiTicket>();
            foreach(var ticket in ticketRepository.Tickets)
            {
                var apiticket = new ApiTicket
                {
                    TicketId = ticket.TicketId,
                    AssignedUser = ticket.AssignedUser?.UserName,
                    AssignedGroup = ticket.AssignedGroup?.Name,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Notes = ticket.Notes,
                    AssignmentStatus = ticket.AssignmentStatus,
                    Opened = ticket.Opened,
                    Closed = ticket.Closed,
                    ParentID = ticket.Parent?.TicketId
                };
                apiTickets.Add(apiticket);
            }
            return apiTickets;
        }
        [HttpGet("{id}")]
        public ApiTicket Get(int id) {
            var ticket = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == id);
            return new ApiTicket
            {
                TicketId = ticket.TicketId,
                AssignedUser = ticket.AssignedUser?.UserName,
                AssignedGroup = ticket.AssignedGroup?.Name,
                Title = ticket.Title,
                Description = ticket.Description,
                Notes = ticket.Notes,
                AssignmentStatus = ticket.AssignmentStatus,
                Opened = ticket.Opened,
                Closed = ticket.Closed,
                ParentID = ticket.Parent?.TicketId
            };
        }
        [HttpPost]
        public async Task<Ticket> Post([FromBody] ApiTicket ticket) { // finish post
            var user = await userManager.FindByNameAsync(ticket.AssignedUser);
            var group = groupRepository.AssignmentGroups.FirstOrDefault(g => g.Name.Equals(ticket.AssignedGroup, StringComparison.OrdinalIgnoreCase));
            var newTicket = new Ticket
            {
                AssignedUser = user,
                AssignedGroup = group,
                Opened = DateTime.Now,
                Title = ticket.Title,
                Description = ticket.Description
            };
            ticketRepository.SaveTicket(newTicket);
            return newTicket;
        }
        [HttpPost("{id}")]
        public ApiTicket Post(int id, [FromBody] string description) // Creates a new note on a ticket
        {
            var ticket = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == id);
            ticketRepository.CreateNote(description, ticket);
            return new ApiTicket
            {
                TicketId = ticket.TicketId,
                AssignedUser = ticket.AssignedUser?.UserName,
                AssignedGroup = ticket.AssignedGroup?.Name,
                Title = ticket.Title,
                Description = ticket.Description,
                Notes = ticket.Notes,
                AssignmentStatus = ticket.AssignmentStatus,
                Opened = ticket.Opened,
                Closed = ticket.Closed,
                ParentID = ticket.Parent?.TicketId
            };
        }
        [HttpPut]
        public Ticket Put([FromBody] Ticket ticket)
        {
            ticketRepository.SaveTicket(ticket);
            return ticket;
        }
        [HttpPatch("{id}")]
        public StatusCodeResult Patch(int id, [FromBody]JsonPatchDocument<Ticket> patch)
        {
            var ticket = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == id);
            if(ticket != null)
            {
                patch.ApplyTo(ticket);
                ticketRepository.SaveTicket(ticket);
                return Ok();
            }
            return NotFound();
        }
        [HttpDelete("{id}")]
        public void Delete(int id) => ticketRepository.DeleteTicket(id);
    }
}
