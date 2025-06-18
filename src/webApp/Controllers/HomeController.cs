using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Authorize]
    public class HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHubContext<EventGridNotificationHub> hubContext)
        : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> SendToEventGrid([FromBody] string message)
        {
            if (message == null)
                return Json(new { success = false, message = "Message cannot be null." });
                
            using var httpClient = new HttpClient();
            var content = new StringContent(message, System.Text.Encoding.UTF8, "application/json");
            try
            {
                var response = await httpClient.PostAsync($"{configuration["AzureFunctions:Url"]}/api/SendTextToEventGrid?code={configuration["AzureFunctions:Key"]}", content);
                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, message = "Data sent to backend." });
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed to send data to Azure Function: {e.Message}");
                return Json(new { success = false, message = "Failed to send data to backend." });
            }
            
            return Json(new { success = false, message = "Failed to send data to backend." });
        }

        [HttpPost]
        [AllowAnonymous]
        // Endpoint as a webhook for Event Grid
        public async Task<IActionResult> MessageFromEventGrid()
        {
            var req = HttpContext.Request;
            var events = await BinaryData.FromStreamAsync(req.Body);
            logger.LogInformation($"Received events: {events}");
            
            var eventGridEvents = EventGridEvent.ParseMany(events);
            foreach (var eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.EventType == "EventGrid.CustomEvent")
                {
                    var eventData = eventGridEvent.Data.ToString();
                    await hubContext.Clients.All.SendAsync("ReceiveMessage", eventData);
                    
                }
            }
                
            return Ok();
        }
    }
}