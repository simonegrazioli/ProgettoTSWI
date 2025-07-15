using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProgettoTSWI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult ApproveRequests() => View("../AdminPages/ApproveRequests");

        public IActionResult DeleteReviews() => View("../AdminPages/DeleteReviews");

        public IActionResult DeleteUsers() => View("../AdminPages/DeleteUsers");

        public IActionResult ManageEvents() => View("../AdminPages/ManageEvents");
    }
}
