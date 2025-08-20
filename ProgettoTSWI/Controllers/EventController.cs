using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Models;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;


namespace ProgettoTSWI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("Event/[action]")]
    public class EventController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // AGGIUNGI QUESTA RIGA - passa il cookie anche per le richieste GET
                client.DefaultRequestHeaders.Add("Cookie", Request.Headers["Cookie"].ToString());

                var response = await client.GetAsync("https://localhost:7087/api/Api/events");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["Message"] = "Non è stato ancora approvato alcun evento";
                    return View(new List<Event>());
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Si è verificato un errore nel recupero degli eventi";
                    return View("Error");
                }

                var events = await response.Content.ReadFromJsonAsync<List<Event>>() ?? new List<Event>();

                // Ottieni l'ID utente corrente
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Se l'utente è loggato, verifica le partecipazioni
                if (!string.IsNullOrEmpty(userId))
                {
                    // AGGIUNGI IL COOKIE ANCHE QUI
                    var partecipationsResponse = await client.GetAsync($"https://localhost:7087/api/Api/participations/user/{userId}");

                    if (partecipationsResponse.IsSuccessStatusCode)
                    {
                        var partecipations = await partecipationsResponse.Content.ReadFromJsonAsync<List<Participation>>();

                        // Aggiungi informazioni di partecipazione a ogni evento
                        foreach (var e in events)
                        {
                            e.UserPartecipa = partecipations?.Any(p => p.ParticipationEventId == e.EventId) ?? false;
                        }
                    }
                }

                return View(events);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Errore: {ex.Message}";
                return View("Error");
            }
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyEvent()
        {

            var client = _httpClientFactory.CreateClient();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int id = int.Parse(userId);//in teoria con authorize non può essere nullo qui

            client.DefaultRequestHeaders.Add("Cookie", Request.Headers["Cookie"].ToString());
            var response = await client.GetAsync($"https://localhost:7087/api/Api/MyEvents?id={id}");

            if (!response.IsSuccessStatusCode)
                return View("Error");
            //li ritorna la lista di eventi organizzati o proposti da quell'utente 
            var myEvents = await response.Content.ReadFromJsonAsync<List<Event>>() ?? new List<Event>();

            // Aggiungi messaggio se non ci sono eventi
            if (!myEvents.Any())
            {
                TempData["Message"] = "Non hai ancora proposto alcun evento.";
            }

            return View(myEvents);
        }

        [HttpGet]
        public IActionResult CreateForm()
        {
            return View(); // Passa un modello vuoto al form
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(EventCreation model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            model.OrganizerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("Cookie", Request.Headers["Cookie"].ToString());
            var response = await client.PostAsJsonAsync(
                "https://localhost:7087/api/Api/createEvent",
                model);

            Console.WriteLine($"Status Code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Ops, qualcosa è andato storto!";
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Errore API: {error}");
                return RedirectToAction("CreateForm");
            }

            TempData["Message"] = "Evento proposto con successo! Sarà visibile dopo approvazione.";
            return RedirectToAction("Index");
        }

        [HttpGet("InfoEvent/{idEvent:int}")]
        public async Task<IActionResult> InfoEvent(int idEvent)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://localhost:7087/api/Api/InfoEvent?idEvent={idEvent}");

                if (!response.IsSuccessStatusCode)
                {
                    
                    return View("Error");
                }

                // Modificato per usare EventWithParticipants
                var eventWithParticipants = await response.Content.ReadFromJsonAsync<EventWithParticipants>();

                if (eventWithParticipants == null)
                {
                    TempData["Message"] = "Evento non trovato";
                    return View(new EventWithParticipants
                    {
                        Event = new Event(),
                        ParticipantsAka = new List<string>()
                    });
                }

                return View(eventWithParticipants);
            }
            catch (Exception)
            {
                
                return View("Error");
            }
        }

        [HttpGet("ViewReviews/{eventId:int}")]
        public async Task<IActionResult> ViewReviews(int eventId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://localhost:7087/api/Api/GetEventReviews?eventId={eventId}");

                if (!response.IsSuccessStatusCode)
                    return RedirectToAction("InfoEvent", new { id = eventId });

                var reviews = await response.Content.ReadFromJsonAsync<List<SeeReviewModel>>();
                return View("EventReviews", reviews ?? new List<SeeReviewModel>());
            }
            catch
            {
                return RedirectToAction("InfoEvent", new { id = eventId });
            }
        }


        [HttpGet("Review/{id:int}")] // Specifica esplicitamente il tipo del parametro
        public async Task<IActionResult> Review(int id)
        {
            

            try
            {
                var client = _httpClientFactory.CreateClient();

                var response = await client.GetAsync($"https://localhost:7087/api/Api/events/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Errore API: {response.ReasonPhrase}");
                    return RedirectToAction("Index");
                }

                var evento = await response.Content.ReadFromJsonAsync<Event>();
               

                return View(new ReviewModel
                {
                    EventId = id,
                    EventName = evento?.EventName ?? "N/D"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ECCEZIONE: {ex}");
                throw; // Rilancia per vedere l'errore nella UI
            }
     
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Partecipa(int id)
        {
            try
            {
                // Ottieni l'ID utente dai claim
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Message"] = "Utente non riconosciuto";
                    return RedirectToAction("Index");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Cookie", Request.Headers["Cookie"].ToString());
                var response = await client.PostAsJsonAsync(
                    "https://localhost:7087/api/Api/participations/confirm", new
                    {
                        EventId = id,
                        UserId = userId
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Partecipazione registrata con successo!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = "Errore durante la partecipazione: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex}");
                TempData["Message"] = "Errore durante l'operazione";
            }

            return RedirectToAction("Index");
        }

       

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> NonPartecipa(int eventId)
        {
            
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Message"] = "Utente non riconosciuto";
                    return RedirectToAction("Index");
                }

               
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Cookie", Request.Headers["Cookie"].ToString());
                var response = await client.PostAsJsonAsync(
                    "https://localhost:7087/api/Api/participations/delete",
                    new
                    {
                        EventId = eventId,
                        UserId = userId
                    });

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Errore API: {(int)response.StatusCode} - {errorContent}";
                }
                else
                {
                    TempData["Message"] = "Partecipazione rimossa con successo!";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Errore: {ex.Message}";
            }

            return RedirectToAction("Index");
        }


        [HttpPost("SubmitReview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(ReviewModel model)
        {
            Console.WriteLine("nel secondo review");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _httpClientFactory.CreateClient();
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Aggiungi l'userId al modello
                model.UserId = int.Parse(userId);
                client.DefaultRequestHeaders.Add("Cookie", Request.Headers["Cookie"].ToString());
                var response = await client.PutAsJsonAsync(
                    "https://localhost:7087/api/Api/participations/review",
                    model);

               

                if (!response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Recensione fallita!";
                    return RedirectToAction("Index");
                }
               
                TempData["SuccessMessage"] = "Recensione salvata con successo!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
                ModelState.AddModelError("", "Errore imprevisto");
                return View(model);
            }
        }
    }
}
