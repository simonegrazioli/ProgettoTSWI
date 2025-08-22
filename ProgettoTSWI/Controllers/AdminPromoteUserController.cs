using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Json;
using System.Net;

namespace ProgettoTSWI.Controllers
{


    [Authorize(Roles = "Admin")]
    public class AdminPromoteUserController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        public AdminPromoteUserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Back()
        {
            return View("../Home/Admin");
        }


        // Ricevo id che devo promuovere a 
        [HttpPost]
        public async Task<IActionResult> PromoteUser(int[] selectedUser)
        {
            Console.WriteLine(selectedUser);
            if (selectedUser == null)
            {
                TempData["ErrorMessage"] = "Nessun utente selezionato.";
                return View("../Home/Admin");
            }

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
                idSelected = selectedUser,
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"
            );//metto le info da passare alla richiesta nel body in formato json

            var jsonString = await jsonContent.ReadAsStringAsync();
            Console.WriteLine(jsonString);
            HttpResponseMessage response;

            response = await client.PostAsync("https://localhost:7087/api/AdminPromoteUserAPI/Promote", jsonContent);


            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var msg = JsonConvert.DeserializeObject<dynamic>(result);
                TempData["SuccessMessage"] = msg.message;
            }
            else
            {
                TempData["ErrorMessage"] = response.StatusCode;
            }

            return View("../Home/Admin");
        }
    }
}