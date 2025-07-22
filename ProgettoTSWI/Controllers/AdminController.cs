using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Data;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using ProgettoTSWI.Models;
using Newtonsoft.Json;


namespace ProgettoTSWI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        //private readonly ApplicationDbContext _context;

        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> ApproveRequests()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7087/api/AdminAPI/unapproved");
            Console.WriteLine($"StatusCode: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);
            var unapprovedEvents = JsonConvert.DeserializeObject<List<Event>>(json);

            return View("../AdminPages/ApproveRequests", unapprovedEvents);
        }
        //public AdminController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}
        //public async Task<IActionResult> ApproveRequests() {

        //    var  proposedEvents =await _context.Events.Where(e => e.IsApproved == false).ToListAsync();

        //    return View("../AdminPages/ApproveRequests", proposedEvents);
        //}

        


        //public async Task<IActionResult> DeleteReviews(){
        //    var reviews = await _context.Participations.Where(p => p.ParticipationReview != "").ToListAsync();


        //    return View("../AdminPages/DeleteReviews", reviews);
        //}

        //public async Task<IActionResult> DeleteUsers()
        //{
        //    var users =await  _context.Users.Where(u => u.Ruolo == "User").ToListAsync();


        //    return View("../AdminPages/DeleteUsers", users);
        //}

        //public async Task<IActionResult> ManageEvents() //passo la lista degli eventi confermati per i form di eliminazione e modifica
        //{
        //    var confirmedEvents = await _context.Events.Where(e => e.IsApproved == true).ToListAsync();

        //    return View("../AdminPages/ManageEvents", confirmedEvents);
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Logout()
        //{
        //    // Questo elimina il cookie e tutti i claims dell'utente
        //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        //    // Redirect alla home o alla pagina di login
        //    return RedirectToAction("Index", "Home");
        //}

    }
}
