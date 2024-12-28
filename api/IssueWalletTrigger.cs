using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Sebug.Function
{
    public class IssueWalletTrigger
    {
        private readonly ILogger<IssueWalletTrigger> _logger;

        public IssueWalletTrigger(ILogger<IssueWalletTrigger> logger)
        {
            _logger = logger;
        }

        [Function("IssueWalletTrigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string firstName = req.Form["first_name"].FirstOrDefault() ?? String.Empty;
            return new OkObjectResult("Wallet for " + firstName);
        }
    }
}
