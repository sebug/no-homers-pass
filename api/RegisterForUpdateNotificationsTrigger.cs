using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sebug.Function.Models;

namespace Sebug.Function
{
    public class RegisterForUpdateNotificationsTrigger
    {
        private readonly ILogger<RegisterForUpdateNotificationsTrigger> _logger;

        public RegisterForUpdateNotificationsTrigger(ILogger<RegisterForUpdateNotificationsTrigger> logger)
        {
            _logger = logger;
        }

        [Function("RegisterForUpdateNotificationsTrigger")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            using var sr = new StreamReader(req.Body);
            string pushBodyString = await sr.ReadToEndAsync();
            PushToken? pushToken = null;
            if (!String.IsNullOrEmpty(pushBodyString))
            {
                pushToken = JsonSerializer.Deserialize<PushToken>(pushBodyString);
            }
            string? authToken = null;
            if (req.Headers != null && req.Headers.Authorization.Any())
            {
                authToken = req.Headers.Authorization.First();
            }
            string? deviceLibraryIdentifier = req.Query["deviceLibraryIdentifier"];
            string? passTypeIdentifier = req.Query["passTypeIdentifier"];
            string? serialNumber = req.Query["serialNumber"];
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!" + pushToken?.pushToken + ". Auth is " + authToken +
            "parameters are: " + deviceLibraryIdentifier + ", " + passTypeIdentifier + ", " + serialNumber);
        }
    }
}
