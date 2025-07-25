using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Json;

namespace ProgettoTSWI.Controllers
{   


    [Authorize(Roles = "Admin")]
    public class ApproveRequestController : Controller
    {
        //private readonly ApplicationDbContext _context;

        //public ApproveRequestController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        private readonly IHttpClientFactory _httpClientFactory;
        public ApproveRequestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Back()
        {
            return View("../Home/Admin");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmRefuseRequest(int[] selectedRequests, string actionType)
        {
            if (selectedRequests == null || selectedRequests.Length == 0)
            {
                TempData["ErrorMessage"] = "Nessun evento selezionato.";
                return View("../Home/Admin");
            }

            var client = _httpClientFactory.CreateClient();

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
                //var request = new HttpRequestMessage
                //{
                //    Method = HttpMethod.Post,
                //    RequestUri = new Uri("https://localhost:7087/api/ApproveRequestAPI/confirm"),
                //    Content = jsonContent // StringContent con JSON
                //};

                //response = await client.SendAsync(request);

                response = await client.PostAsync("https://localhost:7087/api/ApproveRequestAPI/confirm", jsonContent);

            }
            else if (actionType == "refuse")
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri("https://localhost:7087/api/ApproveRequestAPI/refuse"),
                    Content = jsonContent // StringContent con JSON
                };

                response = await client.SendAsync(request);


                //response = await client.PostAsync("https://localhost:7087/api/ApproveRequestAPI/refuse", jsonContent);
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

    //    [HttpPost]
    //    public async Task<IActionResult> ConfirmRefuseRequest(int[] selectedRequests, string actionType)
    //    {
    //        try
    //        {
    //            if(selectedRequests == null || selectedRequests.Count() == 0)
    //            {
    //                TempData["ErrorMessage"] = "Nessun evento selezionato da confermare";
    //                return View("../Home/Admin");
    //            }
    //            if (actionType == "approve")
    //            {
    //                var eventToConfirm = await _context.Events.Where(e => selectedRequests.Contains(e.EventId)).ToListAsync();

    //                foreach (var ev in eventToConfirm)
    //                {
    //                    ev.IsApproved = true;
    //                    _context.Entry(ev).Property(e => e.IsApproved).IsModified = true;

    //                    // creo la partecipazione tra l'organizzatore e l'evento
    //                    var participation = new Participation
    //                    {
    //                        ParticipationEventId = ev.EventId,
    //                        ParticipationUserId = ev.OrganizerId
    //                    };

    //                    _context.Participations.Add(participation);
    //                }

    //                await _context.SaveChangesAsync();

    //                TempData["SuccessMessage"] = $"Confermate qta:{eventToConfirm.Count} eventi con successo";
    //            }
    //            else if (actionType == "refuse")
    //            {
    //                var eventToRefuse = await _context.Events.Where(ev => selectedRequests.Contains(ev.EventId)).ToListAsync();

    //                _context.Events.RemoveRange(eventToRefuse);
    //                await _context.SaveChangesAsync();

    //                TempData["SuccessMessage"] = $"Rifiutati qta:{eventToRefuse.Count} eventi con successo";

    //            }


    //        }
    //        catch (Exception ex)
    //        {
    //            TempData["ErrorMessage"] = "Si è verificato un errore durante l'approvazione/rifiuto degli eventi";
    //        }

    //        return View("../Home/Admin");
    //    }
    //}
}
