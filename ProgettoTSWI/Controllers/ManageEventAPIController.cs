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
    [Authorize(Roles = "Admin")]
    public class ManageEventAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ManageEventAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        [HttpPost("preEdit")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> PreEditEvent([FromBody] int eventToUp)
        {
            try
            {
                var eventToUpdate = await _context.Events
                    .FirstOrDefaultAsync(e => e.EventId == eventToUp);

                if (eventToUpdate == null)
                    return NotFound(new { message = "Evento non trovato." });

                return Ok(eventToUpdate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno nel recupero dell'evento.", details = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> EditEvent([FromBody] EventJson eventUpdated)
        {
            try
            {
                var existingEvent = await _context.Events.FindAsync(eventUpdated.EventId);
                if (existingEvent == null)
                    return NotFound(new { message = "Evento non trovato" });

                existingEvent.EventName = eventUpdated.EventName;
                existingEvent.EventDate = eventUpdated.EventDate;
                existingEvent.EventLocation = eventUpdated.EventLocation;
                existingEvent.EventPrice = eventUpdated.EventPrice;

                _context.Events.Update(existingEvent);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Evento aggiornato con successo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore durante l'update", details = ex.Message });
            }
        }
    }
}

