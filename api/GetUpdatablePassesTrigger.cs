using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sebug.Function.Models;

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
            string previousLastUpdated = req.Query["passesUpdatedSince"].FirstOrDefault() ?? String.Empty;
            _logger.LogError("C# HTTP trigger function processed a request for device library identifier " + deviceLibraryIdentifier);
            var passesUpdatedSince = GetPassesUpdatedSince(deviceLibraryIdentifier, previousLastUpdated);
            if (passesUpdatedSince == null)
            {
                return new NoContentResult();
            }
            return new OkObjectResult(passesUpdatedSince);
        }

        private SerialNumbers? GetPassesUpdatedSince(string deviceLibraryIdentifier, string previousLastUpdated)
        {
            var passStorageProvider = PassStorageProvider.GetFromEnvironment();
            var deviceLibrariesTableClient = passStorageProvider.GetTableClient("deviceLibraries");
            var device = deviceLibrariesTableClient.GetEntityIfExists<TableEntity>("prod", deviceLibraryIdentifier);
            if (device == null || !device.HasValue)
            {
                _logger.LogError("Could not find device library identifier " + deviceLibraryIdentifier);
                return null;
            }
            var deviceToPassTableClient = passStorageProvider.GetTableClient("deviceToPass");
            var searchResults = deviceToPassTableClient.Query<TableEntity>(filter: "DeviceLibraryIdentifier eq '" +
                deviceLibraryIdentifier.Replace("'", String.Empty) + "'");
            var result = new List<TableEntity>();
            DateTimeOffset? referenceDate = null;
            if (!String.IsNullOrWhiteSpace(previousLastUpdated) && DateTimeOffset.TryParse(previousLastUpdated, out var dt))
            {
                referenceDate = dt;
            }
            if (searchResults == null || !searchResults.Any())
            {
                _logger.LogError("No Search results");
            }
            else
            {
                _logger.LogError("Found " + searchResults.Count() + " search results");
            }
            DateTimeOffset? lastUpdated = referenceDate;
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
                            if (!referenceDate.HasValue || !pass.Value.Timestamp.HasValue || pass.Value.Timestamp.Value >= referenceDate)
                            {
                                if (pass.Value.Timestamp.HasValue && (!lastUpdated.HasValue || pass.Value.Timestamp.Value > lastUpdated.Value))
                                {
                                    lastUpdated = pass.Value.Timestamp.Value;
                                }
                                result.Add(pass.Value);
                            }
                        }
                    }
                }
            }
            if (result.Any() && lastUpdated.HasValue)
            {
                return new SerialNumbers(
                    result.Select(r => r.RowKey).ToList(),
                    lastUpdated.Value.ToString("yyyy-MM-dd") + "T" + lastUpdated.Value.ToString("HH:mm")
                );
            }
            return null;
        }
    }
}
