

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Models;
using System.Net.Http;
using System.Security.Claims;

[Authorize]
public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public IActionResult Index()
    {
        var userName = User.FindFirstValue(ClaimTypes.Name);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        ViewBag.UserName = userName;
        ViewBag.UserEmail = userEmail;

        return View();
    }

    [Authorize]
    public IActionResult GetEvents()
    {
        // Semplicemente reindirizza all'action Index del EventController
        return RedirectToAction("Index", "Event");
    }

    [Authorize]
    public IActionResult OrganizeEvent()
    {
        // Semplicemente reindirizza all'action MyEvent del EventController
        return RedirectToAction("MyEvent", "Event");
    }

    [Authorize]
    public IActionResult GetUserInfo()
    {
        // Semplicemente reindirizza all'action Index del UserController
        return RedirectToAction("Index", "User");
    }
}