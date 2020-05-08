using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TicketTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace TicketTracker.Controllers
{
    public class AdminController : Controller
    {
        private ITicketRepository ticketRepository;
        private IGroupRepository groupRepository;
        private UserManager<AppUser> userManager;
        public AdminController(ITicketRepository trepo, IGroupRepository grepo, UserManager<AppUser> usr)
        {
            ticketRepository = trepo;
            groupRepository = grepo;
            userManager = usr;
        }
        // Group actions
        public IActionResult Groups() => View(groupRepository.AssignmentGroups);
        public IActionResult CreateGroup() => View("EditGroup", new AssignmentGroup());
        public IActionResult EditGroup(int groupId) => View(groupRepository.AssignmentGroups.FirstOrDefault(g => g.Id == groupId));
        [HttpPost]
        public IActionResult EditGroup(AssignmentGroup group)
        {
            if (ModelState.IsValid)
            {
                groupRepository.SaveAssignmentGroup(group);
                return RedirectToAction(nameof(Groups));
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public IActionResult DeleteGroup(int groupId)
        {
            groupRepository.DeleteAssignmentGroup(groupId);
            return RedirectToAction(nameof(Groups));
        }
        // Ticket Actions
        public IActionResult Tickets() => View(ticketRepository.Tickets);
        public IActionResult EditTicket(int ticketId) => View(ticketRepository.Tickets.FirstOrDefault(t => t.TicketId == ticketId));
        [HttpPost]
        public IActionResult EditTicket(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticketRepository.SaveTicket(ticket);
                return RedirectToAction(nameof(Tickets));
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public IActionResult DeleteTicket(int ticketId)
        {
            ticketRepository.DeleteTicket(ticketId);
            return RedirectToAction(nameof(Tickets));
        }
        // User Actions
        public IActionResult Users() => View(userManager.Users);
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if(user != null)
            {
                user.AssigmentGroups = groupRepository.AssociatedGroups(user);
                return View(user);
            }
            else
            {
                return RedirectToAction(nameof(Users));
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(string id, string assignmentgroup)
        {
            var user = await userManager.FindByIdAsync(id);
            var group = groupRepository.AssignmentGroups.FirstOrDefault(g => g.Name.Equals(assignmentgroup, StringComparison.OrdinalIgnoreCase));
            if(user != null && group != null)
            {
                groupRepository.AddUser(user, group);
                return RedirectToAction(nameof(Users));
            }
            else
            {
                ModelState.AddModelError("", "Could not find either user or group");
                user.AssigmentGroups = groupRepository.AssociatedGroups(user);
                return View("EditUser", user);
            }
        }
        [HttpPost]
        public async Task<IActionResult> RemoveGroup(string userid, int groupid)
        {
            var user = await userManager.FindByIdAsync(userid);
            var group = groupRepository.AssignmentGroups.FirstOrDefault(g => g.Id == groupid);
            if(user != null && group != null)
            {
                groupRepository.RemoveUser(user, group);
                return RedirectToAction(nameof(Users));
            }
            else
            {
                ModelState.AddModelError("", "Could not find either user or group");
                user.AssigmentGroups = groupRepository.AssociatedGroups(user);
                return View("EditUser", user);
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if(user != null)
            {
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }         
                }
            }
            else
            {
                ModelState.AddModelError("", "User not found");
            }
            return RedirectToAction(nameof(Users));
        }
    }
}
