using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!" + pushBodyString);
        }
    }
}
