using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using ProgettoTSWI.Data;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using ProgettoTSWI.Models;
namespace ProgettoTSWI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ApproveRequestAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ApproveRequestAPIController(ApplicationDbContext context)
        {
            _context = context;
        }




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

                        // creo partecipazione tra organizzatore ed evento
                        /* 
                            la creazione della partecipazione e l'approvazione di un evento sono azioni dipendenti l'una dall'altra
                            se uno fallisce, allora deve fallire anche l'altro, quindi mettendo entrambe le azioni in una chiamata post
                            posso garantire entrambe le condizioni
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


        //[HttpPost("confirmrefuse")]
        //public async Task<IActionResult> ConfirmRefuseRequest([FromBody] idActionRequest request)
        //{
        //    if (request.idSelected == null || request.idSelected.Length == 0)
        //    {
        //        return BadRequest(new { message = "Nessun evento selezionato." });
        //    }

        //    try
        //    {
        //        if (request.ActionType == "approve")
        //        {
        //            var eventToConfirm = await _context.Events.Where(e => request.idSelected.Contains(e.EventId)).ToListAsync();

        //            foreach (var ev in eventToConfirm)
        //            {
        //                ev.IsApproved = true;
        //                _context.Entry(ev).Property(e => e.IsApproved).IsModified = true;

        //                // Crea partecipazione tra organizzatore ed evento
        //                var participation = new Participation
        //                {
        //                    ParticipationEventId = ev.EventId,
        //                    ParticipationUserId = ev.OrganizerId
        //                };
        //                await _context.Participations.AddAsync(participation);
        //            }

        //            await _context.SaveChangesAsync();
        //            return Ok(new { message = $"{eventToConfirm.Count} eventi approvati con successo." });
        //        }
        //        else if (request.ActionType == "refuse")
        //        {
        //            var eventsToRefuse = await _context.Events
        //                .Where(e => request.idSelected.Contains(e.EventId))
        //                .ToListAsync();

        //            _context.Events.RemoveRange(eventsToRefuse);
        //            await _context.SaveChangesAsync();

        //            return Ok(new { message = $"{eventsToRefuse.Count} eventi rifiutati con successo." });
        //        }
        //        else
        //        {
        //            return BadRequest(new { message = "Tipo di azione non valido. Usa 'approve' o 'refuse'." });
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(500, new { message = "Errore interno durante l'approvazione o il rifiuto degli eventi." });
        //    }
        //}
    }
}
