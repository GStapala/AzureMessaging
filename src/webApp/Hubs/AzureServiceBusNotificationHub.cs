using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WebApp.BackgroundWorkers;

namespace WebApp.Hubs;

public class AzureServiceBusNotificationHub(IMessageReader serviceBusReader) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var latestMessages = serviceBusReader.PeekLatestMessages();
        foreach (var message in latestMessages) 
            await Clients.Caller.SendAsync("ReceiveMessage", message);
        await base.OnConnectedAsync();
    }
}
