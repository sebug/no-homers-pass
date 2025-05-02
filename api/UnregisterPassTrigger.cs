using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Sebug.Function
{
    public class UnregisterPassTrigger
    {
        private readonly ILogger<UnregisterPassTrigger> _logger;

        public UnregisterPassTrigger(ILogger<UnregisterPassTrigger> logger)
        {
            _logger = logger;
        }

        [Function("UnregisterPassTrigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete")] HttpRequest req)
        {
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
            string? deviceLibraryIdentifier = req.Query["deviceLibraryIdentifier"];
            string? passTypeIdentifier = req.Query["passTypeIdentifier"];
            string? serialNumber = req.Query["serialNumber"];

            if (serialNumber == null || deviceLibraryIdentifier == null)
            {
                return new NotFoundObjectResult("Did not find pass");
            }
            if (authToken == null)
            {
                return new UnauthorizedObjectResult("Request Not Authorized");
            }
            var passStorageProvider = PassStorageProvider.GetFromEnvironment();
            var passTableEntity = passStorageProvider.GetPassBySerialNumber(serialNumber);
            if (passTableEntity == null)
            {
                return new UnauthorizedObjectResult("Request Not Authorized");
            }
            var pass = passStorageProvider.MapTableEntityToPass(passTableEntity);
            if (pass.authenticationToken != authToken)
            {
                return new UnauthorizedObjectResult("Request Not Authorized");
            }
            var deviceToPassTableClient = passStorageProvider.GetTableClient("deviceToPass");

            var mapping = deviceToPassTableClient.GetEntityIfExists<TableEntity>("prod", deviceLibraryIdentifier + "_" + serialNumber);
            if (mapping != null && mapping.HasValue)
            {
                deviceToPassTableClient.DeleteEntity(mapping.Value);
                return new OkObjectResult("Device Unregistered");
            }

            return new NoContentResult();
        }
    }
}
