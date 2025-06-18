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
        
        // Send to Event Grid
        var eventGridEvent = new
        {
            Id = Guid.NewGuid().ToString(),
            EventType = "CustomEvent",
            Subject = "SampleSubject",
            Data = JsonConvert.DeserializeObject(message),
            EventTime = DateTime.UtcNow,
            DataVersion = "1.0"
        };
        var eventGridEndpoint = Environment.GetEnvironmentVariable("EventGridEndpoint");
        if (string.IsNullOrEmpty(eventGridEndpoint))
        {
            logger.LogError("Event Grid endpoint is not configured.");
            return;
        }
        using var httpClient = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(new[] { eventGridEvent }), System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(eventGridEndpoint, content);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError($"Failed to send message to Event Grid. Status code: {response.StatusCode}");
            return;
        }
        
        logger.LogInformation($"Sending message:[{message}] to event grid");
    }
}