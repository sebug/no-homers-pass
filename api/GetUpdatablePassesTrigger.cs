using Azure.Data.Tables;
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
            _logger.LogInformation("C# HTTP trigger function processed a request for device library identifier " + deviceLibraryIdentifier);
            var passesUpdatedSince = GetPassesUpdatedSince(deviceLibraryIdentifier, previousLastUpdated);
            return new OkObjectResult(passesUpdatedSince);
        }

        private List<TableEntity> GetPassesUpdatedSince(string deviceLibraryIdentifier, string previousLastUpdated)
        {
            var passStorageProvider = PassStorageProvider.GetFromEnvironment();
            var deviceLibrariesTableClient = passStorageProvider.GetTableClient("deviceLibraries");
            var device = deviceLibrariesTableClient.GetEntityIfExists<TableEntity>("prod", deviceLibraryIdentifier);
            if (device == null || !device.HasValue)
            {
                return new List<TableEntity>();
            }
            var deviceToPassTableClient = passStorageProvider.GetTableClient("deviceToPass");
            var searchResults = deviceToPassTableClient.Query<TableEntity>(filter: "DeviceLibraryIdentifier eq '" +
                deviceLibraryIdentifier.Replace("'", String.Empty) + "'");
            var result = new List<TableEntity>();
            DateTime? referenceDate = null;
            if (!String.IsNullOrWhiteSpace(previousLastUpdated) && DateTime.TryParse(previousLastUpdated, out var dt))
            {
                referenceDate = dt;
            }
            if (searchResults != null)
            {
                var passesTableClient = passStorageProvider.GetTableClient("passes");
                foreach (var deviceToPass in searchResults)
                {
                    int underscoreIdx = deviceToPass.RowKey.IndexOf('_');
                    if (underscoreIdx >= 0)
                    {
                        var serialNumber = deviceToPass.RowKey.Substring(underscoreIdx + 1);
                        var pass = passesTableClient.GetEntityIfExists<TableEntity>("prod", serialNumber);
                        if (pass != null && pass.HasValue && pass.Value != null)
                        {
                            // TODO: filter by date
                            result.Add(pass.Value);
                        }
                    }
                }
            }
            return result;
        }
    }
}
