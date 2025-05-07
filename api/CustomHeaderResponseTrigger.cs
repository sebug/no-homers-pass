using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Sebug.Function
{
    public class CustomHeaderResponseTrigger
    {
        private readonly ILogger<CustomHeaderResponseTrigger> _logger;

        public CustomHeaderResponseTrigger(ILogger<CustomHeaderResponseTrigger> logger)
        {
            _logger = logger;
        }

        [Function("CustomHeaderResponseTrigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectWithAdditionalHeaderResult("Ok object with custom header");
        }
    }

    public class OkObjectWithAdditionalHeaderResult : OkObjectResult
    {
        public OkObjectWithAdditionalHeaderResult(object? value) : base(value)
        {
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.Headers.Add("X-Custom-H1", "Custom header content");
            await base.ExecuteResultAsync(context);
        }
    }
}
