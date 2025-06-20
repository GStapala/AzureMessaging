using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using WebApp.Hubs;

namespace WebApp.BackgroundWorkers;

public class ServiceBusQueueReceiverService(IConfiguration configuration, IHubContext<AzureServiceBusNotificationHub> serviceBusHubContext)
    : IHostedService, IDisposable, IMessageReader
{
    private readonly string _connectionString = configuration["AzureServiceBus:ConnectionString"];
    private readonly string _queueName = configuration["AzureServiceBus:QueueName"];
    private ServiceBusProcessor _processor;

    private ConcurrentQueue<string> Messages { get; } = new ConcurrentQueue<string>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var client = new ServiceBusClient(_connectionString);
        _processor = client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());

        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        await _processor.StartProcessingAsync(cancellationToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        // Event is passed through event grid
        var eventGridEvent = EventGridEvent.Parse(args.Message.Body);
        Messages.Enqueue($"{DateTime.UtcNow}: {eventGridEvent.Data}");
        while (Messages.Count > 5)
            Messages.TryDequeue(out _);
        await serviceBusHubContext.Clients.All.SendAsync("ReceiveMessage", $"{TimeZoneInfo.ConvertTime(eventGridEvent.EventTime, TimeZoneInfo.Local):HH:mm:ss} - {eventGridEvent.Data}");
        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Messages.Enqueue($"ERROR: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null)
        {
            await _processor.DisposeAsync();
        }
    }

    public async void Dispose()
    {
        await _processor.DisposeAsync();
    }

    public IEnumerable<string> PeekLatestMessages()
    {
        var messages = Messages.TakeLast(5);
        return messages;
    }
}