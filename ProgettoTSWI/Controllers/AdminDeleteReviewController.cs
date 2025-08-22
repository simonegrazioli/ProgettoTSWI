using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.Linq.Expressions;
using System.Net;
using System.Text;


namespace ProgettoTSWI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDeleteReviewController : Controller
    {


        private readonly IHttpClientFactory _httpClientFactory;
        public AdminDeleteReviewController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public IActionResult Back()
        {
            return View("../Home/Admin");
        }

        // Ricevo una serie di id, Chiamata di "eliminazione" delle Review e reindirizzamento alla view Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int[] selectedReviews)
        {
            if (selectedReviews == null || selectedReviews.Length == 0)
            {
                TempData["ErrorMessage"] = "Nessuna reviews selezionata";
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
                idSelected = selectedReviews,
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"
            );//metto le info da passare alla richiesta nel body in formato json


            var response = await client.PutAsync("https://localhost:7087/api/AdminDeleteReviewsAPI/deleteReviews", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var msg = JsonConvert.DeserializeObject<dynamic>(result);
                TempData["SuccessMessage"] = msg.message;
            }
            else
            {
                TempData["ErrorMessage"] = "Something goes wrong " + response;
            }
            return View("../Home/Admin");
        }
    }
}