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
    public class AdminApproveRequestController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        public AdminApproveRequestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Back()
        {
            return View("../Home/Admin");
        }

        // Ricevo dalla view degli di e un tipo di azione da fare, in base all'azione faccio una chiamata per approvare o rifiutare e ritorno alla view Admin
        [HttpPost]
        public async Task<IActionResult> ConfirmRefuseRequest(int[] selectedRequests, string actionType)
        {
            if (selectedRequests == null || selectedRequests.Length == 0)
            {
                TempData["ErrorMessage"] = "Nessun evento selezionato.";
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
                idSelected = selectedRequests,
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"
            );//metto le info da passare alla richiesta nel body in formato json


            HttpResponseMessage response;

            if (actionType == "approve")
            {
                response = await client.PostAsync("https://localhost:7087/api/AdminApproveRequestAPI/confirm", jsonContent);
            }
            else if (actionType == "refuse")
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri("https://localhost:7087/api/AdminApproveRequestAPI/refuse"),
                    Content = jsonContent
                };

                response = await client.SendAsync(request);
            }
            else
            {
                TempData["ErrorMessage"] = "Richiesta non valida";

                return View("../Home/Admin");
            }

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