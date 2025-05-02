using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Sebug.Function
{
    public class GetExistingPassTrigger
    {
        private readonly ILogger<GetExistingPassTrigger> _logger;

        public GetExistingPassTrigger(ILogger<GetExistingPassTrigger> logger)
        {
            _logger = logger;
        }

        [Function("GetExistingPassTrigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("Got request to get existing pass");
            string? authToken = null;
            if (req.Headers != null && req.Headers.ContainsKey("X-Authorization"))
            {
                string? authorizationHeader = req.Headers["X-Authorization"];
                if (authorizationHeader != null)
                {
                    int spaceIdx = authorizationHeader.IndexOf(' ');
                    if (spaceIdx >= 0)
                    {
                        authToken = authorizationHeader.Substring(spaceIdx + 1);
                    }
                }
            }
            string passTypeIdentifier = req.Query["passTypeIdentifier"].FirstOrDefault() ?? String.Empty;
            string serialNumber = req.Query["serialNumber"].FirstOrDefault() ?? String.Empty;
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
