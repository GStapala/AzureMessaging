using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Functions;

public class SendTextToEventGrid(ILogger<SendTextToEventGrid> logger)
{
    [Function("SendTextToEventGrid")]
    public async Task Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        logger.LogInformation($"Function SendTextToEventGrid triggered by HTTP request: {req.Method} {req.Path}");
        
        var message = await new StreamReader(req.Body).ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(message))
        {
            logger.LogError("Received empty message");
            return;
        }
        
        var eventGridEndpoint = Environment.GetEnvironmentVariable("EventGridEndpoint");
        var topicKey = Environment.GetEnvironmentVariable("EventGridTopicKey");
        if (string.IsNullOrEmpty(eventGridEndpoint) || string.IsNullOrEmpty(topicKey))
        {
            logger.LogError("Configuration for Event Grid endpoint or topic key is missing.");
            return;
        }

        var uri = new Uri(eventGridEndpoint);
        var azureCredentials = new AzureKeyCredential(topicKey);
        var eventGrid = new EventGridPublisherClient(uri, azureCredentials);

        var eventGridEvent = new EventGridEvent(
            eventType: "EventGrid.CustomEvent",
            subject: "az204",
            data: JsonConvert.DeserializeObject(message),
            dataVersion: "1.0"
        );
        
        var response = await eventGrid.SendEventAsync(eventGridEvent);
        
        
        logger.LogInformation($"Sending message:[{message}] to event grid");
    }
}