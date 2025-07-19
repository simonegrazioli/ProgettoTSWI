using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace progetto_prove.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        // Mostra solo gli eventi approvati
        public async Task<IActionResult> Index()
        {
            var approvedEvents = await _context.Events
                .Where(e => e.IsApproved)
                .Include(e => e.Organizer)
                .Include(e => e.Participations) 
                .ToListAsync();

            return View(approvedEvents);
        }


        // POST: /Event/Join - Partecipa all'evento (senza aprire pagina)
        [HttpPost]
        [Authorize] // solo utenti loggati
        public async Task<IActionResult> Join(int id)
        {
            // Recupero ID utente loggato
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            // Evita doppie partecipazioni, dovrei poterlo togliere!
            bool alreadyJoined = await _context.Participations
                .AnyAsync(p => p.ParticipationEventId == id && p.ParticipationUserId == userId);
            if (!alreadyJoined)
            {
                var participation = new Participation
                {
                    ParticipationEventId = id,
                    ParticipationUserId = userId
                };

                _context.Participations.Add(participation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Leave(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var participation = await _context.Participations
                .FirstOrDefaultAsync(p => p.ParticipationEventId == id && p.ParticipationUserId == userId);

            if (participation != null)
            {
                _context.Participations.Remove(participation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }



        // GET: /Event/Review?id=5 - Mostra form recensione
        [Authorize]
        public IActionResult Review(int id)
        {
            ViewBag.EventId = id;
            return View();
        }

        // POST: /Event/Review - Salva recensione
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Review(int id, string review)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            // Trova la partecipazione esistente
            var participation = await _context.Participations
                .FirstOrDefaultAsync(p => p.ParticipationEventId == id && p.ParticipationUserId == userId);

            if (participation != null)
            {
                participation.ParticipationReview = review;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
