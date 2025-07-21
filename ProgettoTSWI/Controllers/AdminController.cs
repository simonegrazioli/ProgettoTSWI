using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Data;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
namespace ProgettoTSWI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> ApproveRequests() {

            var  proposedEvents =await _context.Events.Where(e => e.IsApproved == false).ToListAsync();

            return View("../AdminPages/ApproveRequests", proposedEvents);
        }

        public async Task<IActionResult> DeleteReviews(){
            var reviews = await _context.Participations.Where(p => p.ParticipationReview != "").ToListAsync();


            return View("../AdminPages/DeleteReviews", reviews);
        }

        public async Task<IActionResult> DeleteUsers()
        {
            var users =await  _context.Users.Where(u => u.Ruolo == "User").ToListAsync();


            return View("../AdminPages/DeleteUsers", users);
        }

        public async Task<IActionResult> ManageEvents() //passo la lista degli eventi confermati per i form di eliminazione e modifica
        {
            var confirmedEvents = await _context.Events.Where(e => e.IsApproved == true).ToListAsync();

            return View("../AdminPages/ManageEvents", confirmedEvents);
        }
    }
}
