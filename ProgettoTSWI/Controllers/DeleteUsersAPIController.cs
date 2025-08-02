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
    public class DeleteUsersAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DeleteUsersAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteUsers([FromBody] idActionRequest request)
        {
            if (request == null || request.idSelected.Length == 0 || request.AdminId == null)
                return BadRequest("Nessun utente selezionato.");

            try
            {
                //trovo tutti gli utenti selezionati da eliminare
                var usersToDelete = await _context.Users
                .Where(u => request.idSelected.Contains(u.Id))
                .ToListAsync();


                //partecipazioni : tolgo le partecipazioni dell'utente eliminato
                var partecipazioniDaRimuovere = await _context.Participations
                    .Where(p => request.idSelected.Contains(p.ParticipationUserId))
                    .ToListAsync();
                _context.Participations.RemoveRange(partecipazioniDaRimuovere);


                //eventi : tolgo gli eventi dell'utente eliminato (se non approvati), se approvati metto come organizer l'admin corrente
                //trova tutti gli eventi organizzati dagli utenti da eliminare
                var eventiUtenti = await _context.Events
                    .Where(e => request.idSelected.Contains(e.OrganizerId))
                    .ToListAsync();


                //if (!string.IsNullOrEmpty(request) && int.TryParse(adminId, out int ConverterOrganizerId))
                //{
                    foreach (var evento in eventiUtenti)
                    {
                        if (!evento.IsApproved)
                        {
                            _context.Events.Remove(evento); // Evento non approvato → lo eliminiamo
                        }
                        else
                        {
                            evento.OrganizerId = (int)request.AdminId; // Evento approvato → lo assegniamo all'admin
                        }
                    }
                //}
                //else
                //{
                //    return StatusCode(400, new { message = "Errore claim" });
                //}

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
