using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProgettoTSWI.Models;
using System.Net;
using System.Security.Claims;
using System.Text;


namespace ProgettoTSWI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminManageEventController : Controller
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
        public AdminManageEventController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Ricevo un oggetto Evento, chiamata per inserire tale oggetto, indirizzamento alla view Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertEvent(Event newEvent)
        {
            try
            {
                var clientHandler = new HttpClientHandler();
                var cookieContainer = new CookieContainer();

                // Prendi il cookie di autenticazione attuale
                if (Request.Cookies.TryGetValue("TempAuthCookie", out var authCookieValue))
                {
                    cookieContainer.Add(new Uri("https://localhost:7087"), new Cookie("TempAuthCookie", authCookieValue));
                }

                clientHandler.CookieContainer = cookieContainer;

                var client = new HttpClient(clientHandler);



                var organizerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(organizerIdClaim) && int.TryParse(organizerIdClaim, out int organizerId))
                {
                    var newEventJson = new EventJson
                    {
                        EventName = newEvent.EventName,
                        EventDate = newEvent.EventDate,
                        EventPrice = newEvent.EventPrice,
                        EventLocation = newEvent.EventLocation,
                        OrganizerId = organizerId,
                        IsApproved = true
                    };

                    var json = JsonConvert.SerializeObject(newEventJson);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("https://localhost:7087/api/AdminManageEventAPI/insert", content);

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

            return View("../Home/Admin");
        }

        // Ricevo un array di id, e faccio una chiamata API per eliminarli
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int[] eventIds)
        {
            if (eventIds == null || !eventIds.Any())
            {
                TempData["ErrorMessage"] = "Seleziona almeno un evento da eliminare.";
                return RedirectToAction("Admin", "AdminHome");
            }

            try
            {

                var clientHandler = new HttpClientHandler();
                var cookieContainer = new CookieContainer();

                // Prendi il cookie di autenticazione attuale
                if (Request.Cookies.TryGetValue("TempAuthCookie", out var authCookieValue))
                {
                    cookieContainer.Add(new Uri("https://localhost:7087"), new Cookie("TempAuthCookie", authCookieValue));
                }

                clientHandler.CookieContainer = cookieContainer;

                var client = new HttpClient(clientHandler);


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
                    RequestUri = new Uri("https://localhost:7087/api/AdminManageEventAPI/delete"),
                    Content = jsonContent 
                };

                var response = await client.SendAsync(request);

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
            }

            return RedirectToAction("Admin", "AdminHome");
        }

        // Faccio una chiamata API e reindirizzo alla view UpdateEvent con i valori dell'evento selezionato che verranno precompilati
        [HttpPost]
        public async Task<IActionResult> PreEditEvent(EventJson eventToUp)
        {
            try
            {
                var clientHandler = new HttpClientHandler();
                var cookieContainer = new CookieContainer();

                // Prendi il cookie di autenticazione attuale
                if (Request.Cookies.TryGetValue("TempAuthCookie", out var authCookieValue))
                {
                    cookieContainer.Add(new Uri("https://localhost:7087"), new Cookie("TempAuthCookie", authCookieValue));
                }

                clientHandler.CookieContainer = cookieContainer;

                var client = new HttpClient(clientHandler);


                var json = JsonConvert.SerializeObject(eventToUp.EventId);
                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://localhost:7087/api/AdminManageEventAPI/preEdit", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var eventData = JsonConvert.DeserializeObject<Event>(responseBody);

                    return View("../AdminPages/UpdateEvent", eventData);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Evento non trovato o errore nell'API.";
                }


                TempData["ErrorMessage"] = "Evento non trovato o errore nell'API.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Errore durante la chiamata all'API."+ex.Message;
            }

            return RedirectToAction("Admin", "AdminHome");
        }

        // Chiamata per l'effettiva modifica dell'evento
        [HttpPost]
        public async Task<IActionResult> EditEvent(Event eventUpdated)
        {
            try
            {

                var clientHandler = new HttpClientHandler();
                var cookieContainer = new CookieContainer();

                // Prendi il cookie di autenticazione attuale
                if (Request.Cookies.TryGetValue("TempAuthCookie", out var authCookieValue))
                {
                    cookieContainer.Add(new Uri("https://localhost:7087"), new Cookie("TempAuthCookie", authCookieValue));
                }

                clientHandler.CookieContainer = cookieContainer;

                var client = new HttpClient(clientHandler);

                var json = JsonConvert.SerializeObject(eventUpdated);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync("https://localhost:7087/api/AdminManageEventAPI/update", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Update effettuato con successo!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Errore durante l'update: {error}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Errore durante la chiamata all'API: {ex.Message}";
            }

            return View("../Home/Admin");
        }
    }
}


