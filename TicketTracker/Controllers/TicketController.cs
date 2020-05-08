using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketTracker.Models;


namespace TicketTracker.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        ITicketRepository ticketRepository;
        IGroupRepository groupRepository;
        UserManager<AppUser> userManager;
        public TicketController(ITicketRepository trepo, IGroupRepository grepo, UserManager<AppUser> usr)
        {
            ticketRepository = trepo;
            groupRepository = grepo;
            userManager = usr;
        }
        private string[] CleanGroups(string[] groups)
        {
            var newgroups = new List<string>();
            foreach(var group in groups)
            {
                if (!String.IsNullOrEmpty(group)) newgroups.Add(group);
            }
            return newgroups.ToArray();
        }
        public IActionResult List(int? ticketId, AssignmentStatus? assignment, string title, string[] groups, string user, int pageNum = 1, int pageSize = 10)
        {
            groups = CleanGroups(groups);
            var preTickets = ticketRepository.Tickets
                    .Where(t => ticketId == null || ticketId.Value == t.TicketId)
                    .Where(t => assignment == null || assignment.Value == t.AssignmentStatus)
                    .Where(t => title == null || t.Title.Contains(title))
                    .Where(t => (groups == null || groups.Length == 0) || groups.Contains(t.AssignedGroup.Name))
                    .Where(t => user == null || t.AssignedUser.UserName.Contains(user));
            var viewmodel = new ListViewModel
            {
                Tickets = preTickets
                    .OrderBy(t => t.TicketId)
                    .Skip((pageNum - 1) * pageSize)
                    .Take(pageSize),
                PageNumber = pageNum,
                PageSize = pageSize,
                TicketID = ticketId,
                AssignmentStatus = assignment,
                Title = title,
                User = user,
                Groups = groups,
                Total = preTickets.Count()
            };
            return View(viewmodel);
        }
        public IActionResult Search(ListViewModel model)
        {
            model.Groups = CleanGroups(model.Groups);
            var preTickets = ticketRepository.Tickets
                .Where(t => model.TicketID == null || model.TicketID.Value == t.TicketId)
                .Where(t => model.AssignmentStatus == null || model.AssignmentStatus.Value == t.AssignmentStatus)
                .Where(t => model.Title == null || t.Title.Contains(model.Title))
                .Where(t => model.Groups == null || model.Groups.Length == 0 || model.Groups.Contains(t.AssignedGroup.Name))
                .Where(t => model.User == null || t.AssignedUser.UserName.Contains(model.User));
            model.PageSize = 10;
            model.PageNumber = 1;
            model.Total = preTickets.Count();
            model.Tickets = preTickets.OrderBy(t => t.TicketId)
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize);
            return View("List", model);
        }
            
        public IActionResult Edit(int ticketId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var ticket = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == ticketId);
            return View(ticket);
        } 
        public IActionResult Create() => View("Edit", new Ticket() { Opened = DateTime.Now });
        [HttpPost]
        public async Task<IActionResult> Edit(Ticket ticket, string userName, string assignmentGroup, string returnUrl, int? parentid)
        {
            if (ModelState.IsValid && parentid == null && !ticket.IsChild)
            {
                var group = groupRepository.AssignmentGroups.FirstOrDefault(g => g.Name.Equals(assignmentGroup, StringComparison.OrdinalIgnoreCase));
                var user = userName != null ? await userManager.FindByNameAsync(userName) : null;
                ticket.AssignedGroup = group;
                ticket.AssignedUser = user;
                if (ticket.AssignedUser != null) ticket.AssignmentStatus = AssignmentStatus.InProgress;
                else ticket.AssignmentStatus = AssignmentStatus.Open;
                ticketRepository.SaveTicket(ticket);
                return RedirectToAction(nameof(Edit), new { ticketId = ticket.TicketId, returnUrl});
            }
            else if (ModelState.IsValid && parentid == null && ticket.IsChild) // Unrelate a ticket
            {
                ticketRepository.RemoveChild(ticket);
                return RedirectToAction(nameof(Edit), new { ticketId = ticket.TicketId, returnUrl });
            }
            else if(ModelState.IsValid && parentid.HasValue) // Relate a ticket
            {
                if(ticket.TicketId == 0)
                {
                    ModelState.AddModelError("", "Please save ticket before relating");
                    return View();
                }
                else
                {
                    var parent = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == parentid.Value);
                    var child = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == ticket.TicketId);
                    if(child.Children.Count > 0)
                    {
                        ModelState.AddModelError("", "Cannot relate a parent ticket");
                    }
                    else
                    {
                        ticket.Parent = parent;
                        ticketRepository.SaveTicket(ticket);
                        return RedirectToAction(nameof(Edit), new { ticketId = ticket.TicketId, returnUrl });
                    }
                }
            }
            var original = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == ticket.TicketId);
            ticket.Notes = original.Notes;
            ticket.Children = original.Children;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> EditAndReturn(Ticket ticket, string userName, string assignmentGroup, string returnUrl, int? parentid)
        {
            if (ModelState.IsValid && parentid == null && !ticket.IsChild)
            {
                var group = groupRepository.AssignmentGroups.FirstOrDefault(g => g.Name.Equals(assignmentGroup, StringComparison.OrdinalIgnoreCase));
                var user = userName != null ? await userManager.FindByNameAsync(userName) : null;
                ticket.AssignedGroup = group;
                ticket.AssignedUser = user;
                if (ticket.AssignedUser != null) ticket.AssignmentStatus = AssignmentStatus.InProgress;
                else ticket.AssignmentStatus = AssignmentStatus.Open;
                ticketRepository.SaveTicket(ticket);
                return Redirect(returnUrl ?? "/Ticket/List");
            }
            else if(ModelState.IsValid && parentid == null && ticket.IsChild) // Unrelate a ticket
            {
                ticketRepository.RemoveChild(ticket);
                return Redirect(returnUrl ?? "/Ticket/List");
            }
            else if (ModelState.IsValid && parentid.HasValue) // Relate a ticket
            {
                if (ticket.TicketId == 0)
                {
                    ModelState.AddModelError("", "Please save ticket before relating");
                    return View("Edit");
                }
                else
                {
                    var parent = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == parentid.Value);
                    var child = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == ticket.TicketId);
                    if (child.Children.Count > 0)
                    {
                        ModelState.AddModelError("", "Cannot relate a parent ticket");
                    }
                    else
                    {
                        ticket.Parent = parent;
                        ticketRepository.SaveTicket(ticket);
                        return Redirect(returnUrl ?? "/Ticket/List");
                    }
                }
            }
            var original = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == ticket.TicketId);
            ticket.Notes = original.Notes;
            ticket.Children = original.Children;
            return View("Edit");
        }
        [HttpPost]
        public async Task<IActionResult> Accept(Ticket ticket, string assignmentGroup)
        {
            if (ticket.TicketId != 0) ticket = ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == ticket.TicketId);
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            var group = groupRepository.AssignmentGroups.FirstOrDefault(g => g.Name.Equals(assignmentGroup, StringComparison.OrdinalIgnoreCase));
            if (user != null && ticket.AssignedUser == null)
            {
                ticket.AssignmentStatus = AssignmentStatus.InProgress;
                ticket.AssignedUser = user;
                ticketRepository.SaveTicket(ticket);
                ticket.AssignedGroup = group;
                return RedirectToAction(nameof(Edit), new { ticketId = ticket.TicketId });
            }
            else
            {
                ModelState.AddModelError("", "Cannot accept ticket at this time");
                return View("Edit", ticket);
            }
        }
        [HttpPost]
        public IActionResult AddNote(Ticket ticket, string note)
        {
            if(ticket.TicketId != 0)
            {
                ticketRepository.CreateNote(note, ticket);
                return RedirectToAction(nameof(Edit), new { ticketId = ticket.TicketId });
            }
            else
            {
                ModelState.AddModelError("", "Please save the ticket before adding notes");
                return View("Edit", ticket);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Resolve(Ticket ticket, string returnUrl, string userName, string assignmentGroup)
        {
            if(ModelState.IsValid && ticket.TicketId != 0)
            {
                var user = await userManager.FindByNameAsync(userName);
                var group = groupRepository.AssignmentGroups.FirstOrDefault(g => g.Name == assignmentGroup);
                ticket.AssignedGroup = group;
                ticket.AssignedUser = user;
                ticket.AssignmentStatus = AssignmentStatus.Resolved;
                ticket.Closed = DateTime.Now;
                ticketRepository.SaveTicket(ticket);
                return RedirectToAction("Edit", new { ticketId = ticket.TicketId, returnUrl });
            }
            else if(ticket.TicketId == 0)
            {
                ModelState.AddModelError("", "Please save ticket before resolving");
                return View("Edit", ticket);
            }
            else
            {
                return View("Edit", ticket);
            }
        }
    }
}
