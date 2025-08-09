using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Models;
using System.Security.Claims;

namespace ProgettoTSWI.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


      
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int id = int.Parse(userId);
            var response = await client.GetAsync($"https://localhost:7087/api/Api/infoUser?id={id}");

            if (!response.IsSuccessStatusCode)
                return View("Error");

            var user = await response.Content.ReadFromJsonAsync<User>();
            return View(user);
        }

        [HttpGet]
        public IActionResult ModifyInfoUser()
        {
            return View(); // Passa un modello vuoto al form
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(ModifyUser user) {
            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Aggiungi l'userId al modello
                user.Id = int.Parse(userId);

                Console.WriteLine("Prima");
                var response = await client.PostAsJsonAsync(
                   "https://localhost:7087/api/Api/EditUser", user);
                
                if (!response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Errore durante l'operazione!";

                    return RedirectToAction("Index");
                }

                TempData["SuccessMessage"] = "Ora partecipi all'evento!";
                return RedirectToAction("Index");
            }
            Console.WriteLine("model state non valido");
            TempData["SuccessMessage"] = "Dati inseriti in modo scoretto";
            return  RedirectToAction("Index");





        }
    }
}
