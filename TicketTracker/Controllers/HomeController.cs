using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TicketTracker.Models;

namespace TicketTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ITicketRepository ticketRepository;
        public HomeController(ITicketRepository tckRepo)
        {
            ticketRepository = tckRepo;
        }
        public IActionResult Index() {
            var model = new HomeViewModel
            {
                OpenTickets = ticketRepository.Tickets.Where(t => t.AssignmentStatus == AssignmentStatus.Open).Count(),
                TotalTickets = ticketRepository.Tickets.Count()
            };
            return View(model);
        }
    }
}
