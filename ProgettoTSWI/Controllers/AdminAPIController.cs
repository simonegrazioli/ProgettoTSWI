using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProgettoTSWI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
namespace ProgettoTSWI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ritorno gli eventi non ancora approvati
        [HttpGet("unapproved")]
        public async Task<IActionResult> GetUnapprovedEvents()
        {
            var proposedEvents = await _context.Events
                .Where(e => e.IsApproved == false)
                .ToListAsync();

            return Ok(proposedEvents);
        }

        // Ritorno tutte le reviews
        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews() { 
            var allReviews= await _context.Participations.Where(p => !string.IsNullOrEmpty(p.ParticipationReview)).ToListAsync();

            return Ok(allReviews);

        }

        // Ritorno tutti gli utenti che non sono admin
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var allUsers =await  _context.Users.Where(u => u.Ruolo == "User").ToListAsync();

            return Ok(allUsers);
        }

        // Ritorno tutti gli eventi confermati
        [HttpGet("confirmedEvents")]
        public async Task<IActionResult> GetConfirmedEvents()
        {
            var confirmedEvents = await _context.Events.Where(e => e.IsApproved == true).ToListAsync();

            return Ok(confirmedEvents);
        }
    }


}