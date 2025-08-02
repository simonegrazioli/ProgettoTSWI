using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;


namespace ProgettoTSWI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DeleteUsersController : Controller
    {
        //private readonly ApplicationDbContext _context;

        //public DeleteUsersController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        private readonly IHttpClientFactory _httpClientFactory;
        public DeleteUsersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult Back()
        {
            return View("../Home/Admin");
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUsers(int[] selectedUsers) //deve essere post perchè vengono eseguite operazioni anche sulle partecipazioni e sugni eventi
        {
            if (selectedUsers == null || selectedUsers.Length == 0)
            {
                TempData["ErrorMessage"] = "Nessun utente selezionato";
                return View("../Home/Admin");
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



                //HttpResponseMessage response;
                //var client = _httpClientFactory.CreateClient();
                var requestBody = new idActionRequest
                {
                    idSelected = selectedUsers,
                    AdminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
                };

                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("https://localhost:7087/api/DeleteUsersAPI/delete",jsonContent);


                if (response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    TempData["SuccessMessage"] = msg;
                }
                else
                {
                    TempData["ErrorMessage"] = "Errore API durante l'eliminazione degli utenti.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Errore durante la chiamata all'API.";
            }

            return View("../Home/Admin");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteUsers(int[] selectedUsers)
        //{
        //    if (selectedUsers == null || selectedUsers.Length == 0)
        //    {
        //        TempData["ErrorMessage"] = "Nessun utente selezionato";
        //        return View("../Home/Admin");
        //    }

        //    try
        //    {
        //        var usersToDelete = await _context.Users.Where(u => selectedUsers.Contains(u.Id)).ToListAsync();

        //        /*

        //        TOGLI PARTECIPAZIONI
        //         eventi proposti da lui ?
        //         */


        //        _context.Users.RemoveRange(usersToDelete);
        //        await _context.SaveChangesAsync();

        //        TempData["SuccessMessage"] = $"Eliminati n.{usersToDelete.Count} utenti con successo";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Si è verificato un errore durante l'eliminazione";
        //    }

        //    return View("../Home/Admin");
        //}

    }
}
