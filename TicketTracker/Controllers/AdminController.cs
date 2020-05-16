using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TicketTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace TicketTracker.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        private ITicketRepository ticketRepository;
        private IGroupRepository groupRepository;
        private RoleManager<IdentityRole> roleManager;
        private UserManager<AppUser> userManager;
        public AdminController(ITicketRepository trepo, IGroupRepository grepo, UserManager<AppUser> usr, RoleManager<IdentityRole> roleMngr)
        {
            ticketRepository = trepo;
            groupRepository = grepo;
            userManager = usr;
            roleManager = roleMngr;
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
                var roles = new List<IdentityRole>();
                foreach(var role in roleManager.Roles)
                {
                    if(await userManager.IsInRoleAsync(user, role.Name))
                    {
                        roles.Add(role);
                    }
                }
                var model = new EditUserViewModel
                {
                    User = user,
                    Roles = roles
                };
                return View(model);
            }
            else
            {
                return RedirectToAction(nameof(Users));
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            var group = groupRepository.AssignmentGroups.FirstOrDefault(g => g.Name.Equals(model.GroupName, StringComparison.OrdinalIgnoreCase));
            var role = model.RoleName != null ? await roleManager.FindByNameAsync(model.RoleName) : null;
            if(user != null)
            {
                if(group != null)
                {
                    groupRepository.AddUser(user, group);
                }
                else if(group == null && model.GroupName != null)
                {
                    ModelState.AddModelError("", "Could not find group");
                }
                if(role != null)
                {
                    var result = await userManager.AddToRoleAsync(user, model.RoleName);
                }
                else if(role == null && model.RoleName != null)
                {
                    ModelState.AddModelError("", "Could not find role");
                }
                if (ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Users));
                }
            }
            else
            {
                ModelState.AddModelError("", "Could not find user");
                return RedirectToAction(nameof(Users));
            }
            user.AssigmentGroups = groupRepository.AssociatedGroups(user);
            var roles = new List<IdentityRole>();
            foreach (var r in roleManager.Roles)
            {
                if (await userManager.IsInRoleAsync(user, r.Name))
                {
                    roles.Add(r);
                }
            }
            model.User = user;
            model.Roles = roles;
            return View("EditUser", model);
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
        public async Task<IActionResult> RemoveRole(string userid, string roleid)
        {
            var user = await userManager.FindByIdAsync(userid);
            var role = await roleManager.FindByIdAsync(roleid);
            if(user != null && role != null)
            {
                await userManager.RemoveFromRoleAsync(user, role.Name);
            }
            return RedirectToAction(nameof(Users));
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
        // Role Actions
        public IActionResult Roles() => View(roleManager.Roles);
        public IActionResult CreateRole() => View();
        [HttpPost]
        public async Task<IActionResult> CreateRole([Required]string name)
        {
            if (ModelState.IsValid)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(name));
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Roles));
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(name);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role != null) await roleManager.DeleteAsync(role);
            return RedirectToAction(nameof(Roles));
        }
    }
}
