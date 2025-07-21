using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Review(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var participation = await _context.Participations
                .FirstOrDefaultAsync(p => p.ParticipationEventId == id && p.ParticipationUserId == userId);

            if (participation == null)
                return Forbid(); // l'utente non ha partecipato

            ViewBag.EventId = id;
            ViewBag.ExistingReview = participation.ParticipationReview;

            return View(); // carica Views/Event/Review.cshtml
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitReview(int id, string review)
        {
            if (string.IsNullOrWhiteSpace(review))
            {
                ViewBag.EventId = id;
                ModelState.AddModelError("", "La recensione non può essere vuota.");
                return View("Review");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var participation = await _context.Participations
                .FirstOrDefaultAsync(p => p.ParticipationEventId == id && p.ParticipationUserId == userId);

            if (participation == null)
                return Forbid();

            participation.ParticipationReview = review;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Recensione salvata con successo!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Questo elimina il cookie e tutti i claims dell'utente
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect alla home o alla pagina di login
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(EventFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var evento = new Event
            {
                EventName = model.EventName,
                EventDate = model.EventDate,
                EventLocation = model.EventLocation,
                EventPrice = model.EventPrice,
                OrganizerId = userId,
                IsApproved = false
            };

            _context.Events.Add(evento);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Evento proposto con successo! Sarà visibile dopo approvazione.";
            return RedirectToAction("Index");
        }



    }
}
