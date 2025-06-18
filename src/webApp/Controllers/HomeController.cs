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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<EventGridNotificationHub> _hubContext;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHubContext<EventGridNotificationHub> hubContext)
        {
            _logger = logger;
            _configuration = configuration;
            _hubContext = hubContext;
        }

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
                var response = await httpClient.PostAsync($"{_configuration["AzureFunctions:Url"]}/api/SendTextToEventGrid?code={_configuration["AzureFunctions:Key"]}", content);
                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, message = "Data sent to backend." });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to send data to Azure Function: {e.Message}");
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
            _logger.LogInformation($"Received message from Event Grid:");

            BinaryData events = await BinaryData.FromStreamAsync(req.Body);
            _logger.LogInformation($"Received events: {events}");
            
            EventGridEvent[] eventGridEvents = EventGridEvent.ParseMany(events);
            foreach (EventGridEvent eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.EventType == "EventGrid.CustomEvent")
                {
                    var eventData = eventGridEvent.Data.ToString();
                    _logger.LogInformation($"Got event from azure event hub {eventData}");
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", eventData);
                    
                }
            }
                
            _logger.LogInformation($"Received message from Event Grid:");
            return Ok();
        }
    }
}