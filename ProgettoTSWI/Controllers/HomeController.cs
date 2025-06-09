//Yo!Simo, scusa se l'ho fatto solo ora ma in sti giorni sono stato pieno ahahh


using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Models;

namespace ProgettoTSWI.Controllers;



public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        Console.WriteLine("Index!");
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
