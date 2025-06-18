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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToEventGrid([FromForm] string message)
        {
            //Send to azure function by httpClient 
            using var httpClient = new HttpClient();
            var content = new StringContent(message, System.Text.Encoding.UTF8, "application/json");
            var url = _configuration["AzureFunctions:Url"];
            
            var response = await httpClient.PostAsync($"{url}/api/SendTextToEventGrid", content);
            if (response.IsSuccessStatusCode) 
                return Json(new { success = true, message = "Data sent to backend." });
                
            _logger.LogError($"Failed to send data to Azure Function: {response.ReasonPhrase}");
            return Json(new { success = false, message = "Failed to send data to backend." });
        }
    }
}
