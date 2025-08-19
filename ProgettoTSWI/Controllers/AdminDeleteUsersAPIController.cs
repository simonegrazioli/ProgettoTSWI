using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace ProgettoTSWI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminDeleteUsersAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminDeleteUsersAPIController(ApplicationDbContext context)
        {
            _context = context;
        }


        // Elimino gli utenti selezionati dall'admin. Nei casi in cui un Utente da eliminare ha partecipazioni o eventi a carico (che impediscono l'eliminazione diretta), occorre rimuovere prima tali riferimenti prima di eliminare l'utente stesso.
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteUsers([FromBody] idActionRequest request)
        {
            if (request == null || request.idSelected.Length == 0 || request.AdminId == null)
                return BadRequest("Nessun utente selezionato.");

            try
            {
                // Trovo tutti gli utenti selezionati da eliminare
                var usersToDelete = await _context.Users
                .Where(u => request.idSelected.Contains(u.Id))
                .ToListAsync();


                // Partecipazioni : tolgo le partecipazioni dell'utente eliminato
                var partecipazioniDaRimuovere = await _context.Participations
                    .Where(p => request.idSelected.Contains(p.ParticipationUserId))
                    .ToListAsync();
                _context.Participations.RemoveRange(partecipazioniDaRimuovere);


                // Eventi : tolgo gli eventi dell'utente eliminato (se non approvati), se approvati metto come organizer l'admin corrente
                // Trova tutti gli eventi organizzati dagli utenti da eliminare
                var eventiUtenti = await _context.Events
                    .Where(e => request.idSelected.Contains(e.OrganizerId))
                    .ToListAsync();


                foreach (var evento in eventiUtenti)
                {
                    if (!evento.IsApproved)
                    {
                        _context.Events.Remove(evento); // Se un evento non è approvato lo eliminiamo
                    }
                    else
                    {
                        evento.OrganizerId = (int)request.AdminId; // Se un evento è approvato metto come organizzatore l'admin corrente
                    }
                }


                _context.Users.RemoveRange(usersToDelete);
                await _context.SaveChangesAsync();

                return Ok($"Eliminati n.{usersToDelete.Count} utenti con successo.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Errore durante l'eliminazione degli utenti.");
            }
        }
    }
}
