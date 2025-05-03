using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Sebug.Function
{
    public class InsertLogTrigger
    {
        private readonly ILogger<InsertLogTrigger> _logger;

        public InsertLogTrigger(ILogger<InsertLogTrigger> logger)
        {
            _logger = logger;
        }

        [Function("InsertLogTrigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogError("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
