using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using ProgettoTSWI.Models;
using Newtonsoft.Json;
using System.Net;


namespace ProgettoTSWI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Chiamata per ricevere gli eventi non approvati e reindirizzamento alla view ApproveRequest
        public async Task<IActionResult> ApproveRequests()
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


            var response = await client.GetAsync("https://localhost:7087/api/AdminAPI/unapproved");

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var json = await response.Content.ReadAsStringAsync();
            var unapprovedEvents = JsonConvert.DeserializeObject<List<Event>>(json);

            return View("../AdminPages/ApproveRequests", unapprovedEvents);
        }

        // Chiamata per ricevere le partecipazioni con reviews non nulle o non vuote e reindirizzamento alla view DeleteReviews
        public async Task<IActionResult> DeleteReviews()
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



            var response = await client.GetAsync("https://localhost:7087/api/AdminAPI/reviews");

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var json = await response.Content.ReadAsStringAsync();
            var reviews = JsonConvert.DeserializeObject<List<Participation>>(json);

            return View("../AdminPages/DeleteReviews", reviews);
        }

        // Chiamata per ricevere gli utenti non admin (solo "User") e reindirizzamento alla view DeleteUsers
        public async Task<IActionResult> DeleteUsers()
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

            var response = await client.GetAsync("https://localhost:7087/api/AdminAPI/users");

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<User>>(json);

            return View("../AdminPages/DeleteUsers", users);
        }

        // Chiamata per ricevere gli Eventi appprovati e reindirizzamento alla view ManageEvents
        public async Task<IActionResult> ManageEvents()
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



            var response = await client.GetAsync("https://localhost:7087/api/AdminAPI/confirmedEvents");

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var json = await response.Content.ReadAsStringAsync();
            var confEvents = JsonConvert.DeserializeObject<List<Event>>(json);

            return View("../AdminPages/ManageEvents", confEvents);
        }


        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Elimina il cookie e tutti i claims dell'utente
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect a Index
            return RedirectToAction("Index", "AdminHome");
        }

    }
}
