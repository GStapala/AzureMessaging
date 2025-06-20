using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize]
public class UserController(
    ILogger<UserController> logger,
    IConfiguration configuration,
    IHubContext<EventGridNotificationHub> hubContext,
    IAzureCreator azureCreator)
    : Controller
{
    public IActionResult Index()
    {
        return View();
    }

}