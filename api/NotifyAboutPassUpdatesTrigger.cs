using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Sebug.Function
{
    public class NotifyAboutPassUpdatesTrigger
    {
        private readonly ILogger<NotifyAboutPassUpdatesTrigger> _logger;

        public NotifyAboutPassUpdatesTrigger(ILogger<NotifyAboutPassUpdatesTrigger> logger)
        {
            _logger = logger;
        }

        [Function("NotifyAboutPassUpdatesTrigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            string deviceLibraryIdentifier = req.Form["device_library_identifier"].FirstOrDefault() ?? String.Empty;
            if (String.IsNullOrEmpty(deviceLibraryIdentifier))
            {
                return new UnauthorizedObjectResult("Cannot notify device");
            }
            var passStorageProvider = PassStorageProvider.GetFromEnvironment();
            var deviceLibrariesTableClient = passStorageProvider.GetTableClient("deviceLibraries");
            var device = deviceLibrariesTableClient.GetEntityIfExists<TableEntity>("prod", deviceLibraryIdentifier);
            if (device == null || !device.HasValue || device.Value == null ||
                    device.Value["PushToken"] == null || String.IsNullOrEmpty(device.Value["PushToken"].ToString()))
            {
                return new UnauthorizedObjectResult("Cannot notify device");
            }

            string pushToken = device.Value["PushToken"].ToString()!;

            _logger.LogInformation("Starting notification.");
            return new OkObjectResult("Notified push token " + pushToken);
        }
    }
}
