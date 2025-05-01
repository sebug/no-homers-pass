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
            if (req.Headers.ContainsKey("Authorization"))
            {
                var authHeader = req.Headers.Authorization.First();
                if (authHeader == null)
                {
                    throw new Exception("Expected auth header by now");
                }
                int applePassIdx = authHeader.IndexOf("ApplePass");
                if (applePassIdx < 0)
                {
                    throw new Exception("Authorization header is not ApplePass");
                }
                int spaceIndex = authHeader.IndexOf(' ');
                if (spaceIndex < 0)
                {
                    throw new Exception("Did not find space after ApplePass");
                }
                authToken = authHeader.Substring(spaceIndex + 1);
            }
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!" + pushToken?.pushToken + ". Auth is " + authToken);
        }
    }
}
