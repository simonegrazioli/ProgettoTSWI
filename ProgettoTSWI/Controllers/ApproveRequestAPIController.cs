using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProgettoTSWI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using ProgettoTSWI.Models;
namespace ProgettoTSWI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]

    public class ApproveRequestAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ApproveRequestAPIController(ApplicationDbContext context)
        {
            _context = context;
        }



        // Confermo la proposta di uno o più eventi proposti dagli utenti "User" creando la partecipazione
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmRequest([FromBody] idActionRequest request)
        {
            if (request.idSelected == null || request.idSelected.Length == 0)
            {
                return BadRequest(new { message = "Nessun evento selezionato." });
            }

            try
            {
                    var eventToConfirm = await _context.Events.Where(e => request.idSelected.Contains(e.EventId)).ToListAsync();

                    foreach (var ev in eventToConfirm)
                    {
                        ev.IsApproved = true;
                        _context.Entry(ev).Property(e => e.IsApproved).IsModified = true;

                        /*
                            Creo partecipazione tra organizzatore ed evento.
                         
                            La creazione della partecipazione e l'approvazione di un evento sono azioni dipendenti l'una dall'altra
                            se uno fallisce, allora deve fallire anche l'altro, quindi mettendo entrambe le azioni in una chiamata post
                            posso garantire entrambe le condizioni.
                         */
                        var participation = new Participation
                        {
                            ParticipationEventId = ev.EventId,
                            ParticipationUserId = ev.OrganizerId
                        };
                        await _context.Participations.AddAsync(participation);
                    }

                    await _context.SaveChangesAsync();
                    return Ok(new { message = $"{eventToConfirm.Count} eventi approvati con successo." });
                
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Errore interno durante l'approvazione  degli eventi." });
            }
        }

        // Rimuovo l'evento o gli eventi proposti da uno o più utenti.
        [HttpDelete("refuse")]
        public async Task<IActionResult> RefuseRequest([FromBody] idActionRequest request)
        {
            if (request.idSelected == null || request.idSelected.Length == 0)
            {
                return BadRequest(new { message = "Nessun evento selezionato." });
            }

            try
            {
                    var eventsToRefuse = await _context.Events
                        .Where(e => request.idSelected.Contains(e.EventId))
                        .ToListAsync();

                    _context.Events.RemoveRange(eventsToRefuse);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = $"{eventsToRefuse.Count} eventi rifiutati con successo." });

            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Errore interno durante il rifiuto degli eventi." });
            }
        }
    }
}
