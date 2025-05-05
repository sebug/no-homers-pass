using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sebug.Function.Models;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
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

            var passStorageProvider = PassStorageProvider.GetFromEnvironment();

            var entry = passStorageProvider.GetPassBySerialNumber(serialNumber);

            if (entry == null)
            {
                return new NotFoundObjectResult("Did not find pass with serial number " + serialNumber);
            }

            if (entry["AccessKey"] == null || entry["AccessKey"].ToString() != authToken)
            {
                return new UnauthorizedObjectResult("Not authorized");
            }

            if (entry.ContainsKey("NameOnBadge") && entry["NameOnBadge"] != null &&
            entry["NameOnBadge"].ToString()!.Contains("Homer", StringComparison.InvariantCultureIgnoreCase))
            {
                return new BadRequestObjectResult("The sign said no Homers!");
            }

            var pass = passStorageProvider.MapTableEntityToPass(entry);

            var settings = PassSettings.GetFromEnvironment();

            var passContentBytes = await new PkPassFileGenerator(pass).Generate(settings.PrivateKeyBytes, settings.PrivateKeyPassword);
        
            return new FileContentResult(passContentBytes,
            "application/zip")
            {
                FileDownloadName = "pass.pkpass"
            };
        }
    }
}
