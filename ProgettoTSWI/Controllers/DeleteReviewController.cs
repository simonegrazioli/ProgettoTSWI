using Azure;
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
    public class DeleteReviewController : Controller
    {
        //private readonly ApplicationDbContext _context;

        //public DeleteReviewController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        private readonly IHttpClientFactory _httpClientFactory;
        public DeleteReviewController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public IActionResult Back()
        {
            return View("../Home/Admin");
        }

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



            //var client = _httpClientFactory.CreateClient();
            
            var requestBody = new idActionRequest
            {
                idSelected = selectedReviews,
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"
            );//metto le info da passare alla richiesta nel body in formato json


            var response = await client.PutAsync("https://localhost:7087/api/DeleteReviewsAPI/deleteReviews", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var msg = JsonConvert.DeserializeObject<dynamic>(result);
                TempData["SuccessMessage"] = msg.message;
            }
            else
            {
                TempData["ErrorMessage"] = "Something goes wrong "+response;
            }
            return View("../Home/Admin");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteReview(int[] selectedReviews)
        //{
        //    if (selectedReviews == null || selectedReviews.Length == 0)
        //    {
        //        TempData["ErrorMessage"] = "Nessuna reviews selezionata";
        //        return View("../Home/Admin");
        //    }

        //    try
        //    {

        //        //BISOGNA FARE UN UPDATE ALLA TABELLA PARTECIPATION AGGIORNANDO PartecipationReviews a "" 
        //        var reviewsToDelete = await _context.Participations.Where(p => selectedReviews.Contains(p.ParticipationId)).ToListAsync();

        //        foreach (var participation in reviewsToDelete)
        //        {
        //            participation.ParticipationReview = string.Empty; // imposto a stringa vuota
        //            _context.Entry(participation).Property(p => p.ParticipationReview).IsModified = true; // genererà: UPDATE Participations SET ParticipationReview = '' WHERE ParticipationId = X, dove X sono le review selezionate nella view
        //        }


        //        await _context.SaveChangesAsync();

        //        TempData["SuccessMessage"] = $"Eliminate qta:{reviewsToDelete.Count} reviews con successo";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Si è verificato un errore durante l'eliminazione della recensione";
        //    }

        //    return View("../Home/Admin");
        //}

    }
}
