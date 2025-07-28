using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;



namespace ProgettoTSWI.Controllers
{
    public class EventJson
    {

        public int EventId { get; set; }

        public string EventName { get; set; }

        public DateTime EventDate { get; set; }

        public decimal? EventPrice { get; set; }

        public string EventLocation { get; set; }

        public bool IsApproved { get; set; } = false;

        public int OrganizerId { get; set; }
    }

    
    [ApiController]
    [Route("api/[controller]")]
    public class ManageEventAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ManageEventAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        //[HttpPost("insert")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> InsertEvent([FromBody] EventJson nEvent)
        //{
        //    try
        //    {
        //        // Recupera l'ID organizzatore dall'utente autenticato
        //        var organizerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //        if (string.IsNullOrEmpty(organizerIdClaim))
        //        {
        //            return Unauthorized(new { message = "ID organizzatore non trovato nell'utente autenticato." });
        //        }

        //        // Provo a convertire in int l'ID organizzatore
        //        if (!int.TryParse(organizerIdClaim, out int organizerId))
        //        {
        //            return BadRequest(new { message = $"ID organizzatore non valido: {organizerIdClaim}" });
        //        }

        //        // Costruisco l'evento da salvare
        //        var newEvent = new Event
        //        {
        //            EventName = nEvent.EventName,
        //            EventDate = nEvent.EventDate,
        //            EventPrice = nEvent.EventPrice,
        //            EventLocation = nEvent.EventLocation,
        //            OrganizerId = organizerId,
        //            IsApproved = true
        //        };

        //        // Aggiungo l'evento al contesto e salvo
        //        await _context.Events.AddAsync(newEvent);
        //        await _context.SaveChangesAsync();

        //        return Ok(new { message = "Evento inserito con successo." });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Logga l'eccezione (se hai un logger) o almeno la stampi per debugging
        //        // _logger.LogError(ex, "Errore durante l'inserimento evento");

        //        return StatusCode(500, new { message = "Errore interno durante l'inserimento dell'evento." });
        //    }
        //}




        //[HttpPost("insertEv")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> InsertEvent([FromBody] Event newEvent)
        //{

        //    try
        //    {
        //        await _context.Events.AddAsync(newEvent); //aggiungo l'evento
        //        await _context.SaveChangesAsync(); //salvo i cambiamenti

        //        return Ok(new { message = "Evento inserito con successo." });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Errore durante inserimento: " + ex.Message);
        //        return StatusCode(500, new { message = ex.Message });
        //    }
        //}

        [HttpPost("insert")]
        public async Task<IActionResult> InsertEvent([FromBody] EventJson newEventJson)
        {
            try
            {

                var newEvent = new Event
                {
                    EventName = newEventJson.EventName,
                    EventDate = newEventJson.EventDate,
                    EventPrice = newEventJson.EventPrice,
                    EventLocation = newEventJson.EventLocation,
                    OrganizerId = newEventJson.OrganizerId,
                    IsApproved = true
                };

                await _context.Events.AddAsync(newEvent);
                int result = await _context.SaveChangesAsync();

                if (result == 0)
                {
                    return StatusCode(500, new { message = "Nessuna riga salvata." });
                }

                return Ok(new { message = "Evento creato con successo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore durante la creazione dell'evento!" });
            }
        }


        [HttpDelete("delete")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEvent([FromBody] idActionRequest request)
        {
            if (request.idSelected == null || request.idSelected.Length == 0)
            {
                return BadRequest(new { message = "Nessun evento selezionato." });
            }

            try
            {
                var participationsToRemove = await _context.Participations
                    .Where(p => request.idSelected.Contains(p.ParticipationEventId))
                    .ToListAsync();

                _context.Participations.RemoveRange(participationsToRemove);

                var eventsToDelete = await _context.Events
                    .Where(e => request.idSelected.Contains(e.EventId))
                    .ToListAsync();

                _context.Events.RemoveRange(eventsToDelete);

                var result = await _context.SaveChangesAsync();

                return Ok(new { message = $"Eliminati {eventsToDelete.Count} evento/i con successo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore durante l'eliminazione: " + ex.Message });
            }
        }

    }
}

