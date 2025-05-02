using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Sebug.Function
{
    public class GetUpdatablePassesTrigger
    {
        private readonly ILogger<GetUpdatablePassesTrigger> _logger;

        public GetUpdatablePassesTrigger(ILogger<GetUpdatablePassesTrigger> logger)
        {
            _logger = logger;
        }

        [Function("GetUpdatablePassesTrigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            string passTypeIdentifier = req.Query["passTypeIdentifier"].FirstOrDefault() ?? String.Empty;
            string deviceLibraryIdentifier = req.Query["deviceLibraryIdentifier"].FirstOrDefault() ?? String.Empty;
            string previousLastUpdated = req.Query["previousLastUpdated"].FirstOrDefault() ?? String.Empty;
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
