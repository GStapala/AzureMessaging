using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> MessageFromEventGrid(string message)
        {
            // Handle the message from Event Grid
            _logger.LogInformation($"Received message from Event Grid: {message}");
            return Json(new { success = true, message = "Message received from Event Grid." });
        }
    }
}