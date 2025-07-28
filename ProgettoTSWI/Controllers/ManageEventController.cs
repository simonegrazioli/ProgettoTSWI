using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;


namespace ProgettoTSWI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageEventController : Controller
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

        private readonly IHttpClientFactory _httpClientFactory;
        public ManageEventController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertEvent(Event newEvent)
        {
            try
            {
                var organizerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(organizerIdClaim) && int.TryParse(organizerIdClaim, out int organizerId))
                {
                    // Prepara oggetto JSON
                    var newEventJson = new EventJson
                    {
                        EventName = newEvent.EventName,
                        EventDate = newEvent.EventDate,
                        EventPrice = newEvent.EventPrice,
                        EventLocation = newEvent.EventLocation,
                        OrganizerId = organizerId,
                        IsApproved = true
                    };

                    var client = _httpClientFactory.CreateClient();
                    var json = JsonConvert.SerializeObject(newEventJson);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("https://localhost:7087/api/ManageEventAPI/insert", content);

                    if (response.IsSuccessStatusCode)
                        TempData["SuccessMessage"] = "Evento creato con successo!";
                    else
                        TempData["ErrorMessage"] = "Errore durante la creazione dell'evento.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Errore: ID organizzatore non valido.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Eccezione durante la creazione dell'evento!";
            }

            return View("../Home/Admin"); // o la tua view destinazione
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int[] eventIds)
        {
            if (eventIds == null || !eventIds.Any())
            {
                TempData["ErrorMessage"] = "Seleziona almeno un evento da eliminare.";
                return RedirectToAction("Admin", "Home");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                var requestBody = new idActionRequest
                {
                    idSelected = eventIds,
                };

                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );//metto le info da passare alla richiesta nel body in formato json

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri("https://localhost:7087/api/ManageEventAPI/delete"),
                    Content = jsonContent // StringContent con JSON
                };

                var response = await client.SendAsync(request);


                // Chiamata all'API REST
                //var response = await client.PostAsync("https://localhost:7087/api/ManageEventAPI/delete", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Evento/i eliminato/i con successo!";
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Errore dall'API: {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Errore interno durante la chiamata all'API.";
                // Logga `ex.Message` per il debug
            }

            return RedirectToAction("Admin", "Home");
        }

    }


    //private readonly ApplicationDbContext _context;

    //public ManageEventController(ApplicationDbContext context)
    //{
    //    _context = context;
    //}


    //[HttpPost]
    //public async Task<IActionResult> InsertEvent(Event newEvent)
    //{
    //    try
    //    {
    //        newEvent.IsApproved = true; // valore da assegnare all'attributo status di un evento in caso in cui viene inserito direttamente dall'admin (non deve essere approvato da nessuno in questo caso)

    //        var organizerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier); // Restituisce stringa o null

    //        // Verifica se il claim esiste e non è vuoto (usa && invece di ||)
    //        if (!string.IsNullOrEmpty(organizerIdClaim))
    //        {
    //            // Converti in int (gestendo eventuali errori)
    //            if (int.TryParse(organizerIdClaim, out int organizerId))
    //            {
    //                newEvent.OrganizerId = organizerId; // Assegna l'ID convertito
    //                _context.Events.Add(newEvent); //aggiungo l'evento
    //                await _context.SaveChangesAsync(); //salvo i cambiamenti

    //                TempData["SuccessMessage"] = "Evento creato con successo!"; //mando un messaggio di feebdack nella pagina 'Admin.cshtml' tramite TempData
    //            }
    //            else
    //            {
    //                TempData["ErrorMessage"] = "Si è verificato un problema con l'id dell'organizzatore durante la creazione dell'evento!";
    //            }
    //        }
    //        else
    //        {
    //            TempData["ErrorMessage"] = "Si è verificato un problema con l'id dell'organizzatore durante la creazione dell'evento!";
    //        }



    //        return View("../Home/Admin"); //reindirizzo a 'Admin.cshtml'
    //    }
    //    catch (Exception ex)
    //    {
    //        TempData["ErrorMessage"] = "Si è verificato un problema durante la creazione dell'evento!";
    //        //ModelState.AddModelError("", $"Errore durante il salvataggio: {ex.Message}");
    //    }

    //    return View("../AdminPages/ManageEvents");
    //}

    //[HttpPost] //devo usare post perche nel form della view method='delete' non è supportato 
    //public IActionResult DeleteEvent(Event eventToDelete)
    //{
    //    try
    //    {
    //        // cerco l'evento da eliminare
    //        //var eventToDelete = _context.Events.FirstOrDefault(e => e.EventId == idEvent);

    //        if (eventToDelete == null)
    //        {
    //            TempData["ErrorMessage"] = "L'evento che si è cercato di eliminare non è stato trovato";
    //            return View("../Home/Admin");
    //        }

    //        // eliminazione effettica evento
    //        _context.Events.Remove(eventToDelete);
    //        _context.SaveChanges();

    //        TempData["SuccessMessage"] = $"Evento eliminato con successo!";
    //    }
    //    catch (Exception ex)
    //    {
    //        TempData["ErrorMessage"] = "Si è verificato un errore durante l'eliminazione";
    //    }

    //    return View("../Home/Admin");
    //}





    /////////////////////////////////////////



    //[HttpPost]
    //    public async Task<IActionResult> DeleteEvent(List<int> eventIds)
    //    {
    //        try
    //        {
    //            if (eventIds == null || !eventIds.Any())
    //            {
    //                TempData["ErrorMessage"] = "Seleziona almeno un evento da eliminare.";
    //                return View("../Home/Admin"); // o la pagina appropriata
    //            }

    //            // trovo le partecipazioni legate agli eventi
    //            var participationsToRemove = await _context.Participations.Where(p => eventIds.Contains(p.ParticipationEventId)).ToListAsync();

    //            // rimuovo le partecipazioni
    //            _context.Participations.RemoveRange(participationsToRemove);

    //            var eventsToDelete = await _context.Events.Where(e => eventIds.Contains(e.EventId)).ToListAsync();

    //            _context.Events.RemoveRange(eventsToDelete);
    //            await _context.SaveChangesAsync();

    //            TempData["SuccessMessage"] = $"{eventsToDelete.Count} evento/i eliminato/i con successo.";
    //        }
    //        catch (Exception ex)
    //        {
    //            TempData["ErrorMessage"] = "Si è verificato un errore durante l'eliminazione";
    //        }
    //        return View("../Home/Admin");
    //    }


    //[HttpPost]
    //public async Task<IActionResult> PreEditEvent(Event EventToUp)
    //{
    //    try
    //    {
    //        var eventToUpdate = await _context.Events.FirstOrDefaultAsync(e => e.EventId==EventToUp.EventId);
    //        return View("../AdminPages/UpdateEvent", eventToUpdate);
    //    }catch(Exception ex)
    //    {
    //        TempData["ErrorMessage"] = "Si è verificato un errore nel recuperare l'evento selezionato per la modifica";
    //    }
    //    return View("../Home/Admin");
    //}

    //[HttpPost]
    //public async Task<IActionResult> EditEvent(Event eventUpdated)
    //{

    //    try
    //    {
    //        //if (eventUpdated == null)
    //        //{
    //        //    TempData["ErrorMessage"] = "L'evento che si è cercato di aggiornare non è stato trovato";
    //        //    return View("../Home/Admin");
    //        //}

    //        //_context.Events.Update(eventUpdated);
    //        //_context.SaveChanges();
    //        var existingEvent =await _context.Events.FindAsync(eventUpdated.EventId);
    //        if (existingEvent == null)
    //        {
    //            TempData["ErrorMessage"] = "Evento non trovato";
    //            return View("../Home/Admin");
    //        }

    //        // update solo campi consentiti
    //        existingEvent.EventName = eventUpdated.EventName;
    //        existingEvent.EventDate = eventUpdated.EventDate;
    //        existingEvent.EventLocation = eventUpdated.EventLocation;
    //        existingEvent.EventPrice = eventUpdated.EventPrice;

    //        _context.Update(existingEvent);
    //        await _context.SaveChangesAsync();

    //        TempData["SuccessMessage"] = $"Update effettuato con successo!";
    //    }
    //    catch (Exception ex)
    //    {
    //        TempData["ErrorMessage"] = "Si è verificato un errore durante l'update";
    //    }
    //    return View("../Home/Admin");
    //}

}


