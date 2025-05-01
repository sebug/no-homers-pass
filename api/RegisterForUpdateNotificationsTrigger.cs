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


            string saAccessKey = Environment.GetEnvironmentVariable("SA_ACCESS_KEY") ??
                throw new Exception("SA_ACCESS_KEY environment variable not defined");

            string saAccountName = Environment.GetEnvironmentVariable("SA_ACCOUNT_NAME") ??
                throw new Exception("SA_ACCOUNT_NAME environment variable not defined");

            string saStorageUri = Environment.GetEnvironmentVariable("SA_STORAGE_URI") ??
                throw new Exception("SA_STORAGE_URI environment variable not defined");

            var deviceLibrariesTableClient = new TableClient(new Uri(saStorageUri),
                "deviceLibraries",
                new TableSharedKeyCredential(saAccountName, saAccessKey));

            deviceLibrariesTableClient.CreateIfNotExists();

            var deviceLibaryEntity = new TableEntity("prod", deviceLibraryIdentifier)
            {
            };
            deviceLibrariesTableClient.UpsertEntity(deviceLibaryEntity);

            var passMappingTableClient = new TableClient(new Uri(saStorageUri),
            "deviceToPass",
            new TableSharedKeyCredential(saAccountName, saAccessKey));
            passMappingTableClient.CreateIfNotExists();

            var passMappingEntity = new TableEntity("prod",
            deviceLibraryIdentifier + "_" + serialNumber)
            {
                { "DeviceLibraryIdentifier", deviceLibraryIdentifier },
                { "SerialNumber", serialNumber },
                { "PassTypeIdentifier", passTypeIdentifier },
                { "AuthToken", authToken ?? String.Empty },
                { "PushToken", pushToken?.pushToken ?? String.Empty }
            };

            passMappingTableClient.UpsertEntity(passMappingEntity);
        }
    }
}
