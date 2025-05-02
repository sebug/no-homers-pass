using System.Text.Json;
using Azure.Data.Tables;
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

            if (serialNumber == null)
            {
                return new NotFoundObjectResult("Did not find pass");
            }
            
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

            _logger.LogInformation("Registering device " + deviceLibraryIdentifier + " for pass type " + passTypeIdentifier + " with serial number " +
            serialNumber + ", auth token is " + authToken);

            RegisterInDatabase(pushToken, deviceLibraryIdentifier,
            passTypeIdentifier, serialNumber, authToken);

            return new OkObjectResult("Welcome to Azure Functions!" + pushToken?.pushToken + ". Auth is " + authToken +
            "parameters are: " + deviceLibraryIdentifier + ", " + passTypeIdentifier + ", " + serialNumber);
        }

        private void RegisterInDatabase(PushToken? pushToken,
            string? deviceLibraryIdentifier,
            string? passTypeIdentifier,
            string? serialNumber,
            string? authToken)
        {
            var passStorageProvider = PassStorageProvider.GetFromEnvironment();

            var deviceLibrariesTableClient = passStorageProvider.GetTableClient("deviceLibraries");
            deviceLibrariesTableClient.CreateIfNotExists();

            var deviceLibaryEntity = new TableEntity("prod", deviceLibraryIdentifier)
            {
                { "PushToken", pushToken?.pushToken ?? String.Empty }
            };
            deviceLibrariesTableClient.UpsertEntity(deviceLibaryEntity);

            var passMappingTableClient = passStorageProvider.GetTableClient("deviceToPass");
            passMappingTableClient.CreateIfNotExists();

            var passMappingEntity = new TableEntity("prod",
            deviceLibraryIdentifier + "_" + serialNumber)
            {
                { "DeviceLibraryIdentifier", deviceLibraryIdentifier },
                { "SerialNumber", serialNumber },
                { "PassTypeIdentifier", passTypeIdentifier },
                { "AuthToken", authToken ?? String.Empty }
            };

            passMappingTableClient.UpsertEntity(passMappingEntity);
        }
    }
}
