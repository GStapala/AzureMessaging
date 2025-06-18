using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class EventGridNotificationHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}