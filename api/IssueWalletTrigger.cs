using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sebug.Function.Models;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            FunctionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string eventName = req.Form["event_name"].FirstOrDefault() ?? String.Empty;
            string firstName = req.Form["first_name"].FirstOrDefault() ?? String.Empty;
            string lastName = req.Form["last_name"].FirstOrDefault() ?? String.Empty;
            string foregroundColorHex = req.Form["foreground_color"].FirstOrDefault() ?? "#000000";
            string backgroundColorHex = req.Form["background_color"].FirstOrDefault() ?? "#ffffff";
            string labelColorHex = req.Form["label_color"].FirstOrDefault() ?? "#000000";
            try
            {

                var serviceClient = CreateTableServiceClient();

                string serialNumber = Guid.NewGuid().ToString().ToUpper();

                string accessKey = Guid.NewGuid().ToString().ToLower().Replace("-", String.Empty);

                var settings = PassSettings.GetFromEnvironment();

                var expiration = DateTimeOffset.Now.AddDays(1);
                expiration = new DateTimeOffset(expiration.Year, expiration.Month, expiration.Day, expiration.Hour,
                expiration.Minute, expiration.Second, TimeSpan.FromHours(2));

                string expirationDate = expiration.ToString("yyyy-MM-ddTHH:mm:sszzz");

                var relevant = DateTimeOffset.Now;
                
                string relevantDate = new DateTimeOffset(relevant.Year, relevant.Month, relevant.Day,
                relevant.Hour, relevant.Minute, relevant.Second, TimeSpan.FromHours(2)).ToString("yyyy-MM-ddTHH:mm:sszzz");

                var eventTicket = new EventTicket(headerFields: new List<Field> { new Field (
                    key : "event",
                    label : "Event",
                    value : eventName
                )
                },
                    primaryFields: new List<Field> { new Field (
                        key : "nameOnBadge",
                        label : "Name",
                        value : firstName + " " + lastName
                    )
                    },
                    secondaryFields: [],
                    auxiliaryFields: [],
                    backFields: new List<Field> {
                        new Field(
                        key : "fullName",
                        label : "Full Name",
                        value : firstName + " " + lastName
                        )
            });

                var barcode = new Barcode(serialNumber.Replace("-", ""),
                "PKBarcodeFormatQR", serialNumber.Replace("-", ""), "utf-8");
                var pass = new Pass("No Homers", settings.PassTypeIdentifier, settings.PassDescription, serialNumber,
                settings.TeamIdentifier, expirationDate, relevantDate, eventTicket, barcode,
                foregroundColor: GetRGBColorTriplet(foregroundColorHex),
                backgroundColor: GetRGBColorTriplet(backgroundColorHex),
                labelColor: GetRGBColorTriplet(labelColorHex),
                authenticationToken: accessKey,
                webServiceURL: settings.APIManagementBaseURL);
                InsertPassInformation(pass);

                var passContentBytes = await new PkPassFileGenerator(pass).Generate(settings.PrivateKeyBytes, settings.PrivateKeyPassword);
                
                return new FileContentResult(passContentBytes,
                "application/zip")
                {
                    FileDownloadName = "pass.pkpass"
                };
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private TableServiceClient CreateTableServiceClient()
        {
            string saAccessKey = Environment.GetEnvironmentVariable("SA_ACCESS_KEY") ??
                throw new Exception("SA_ACCESS_KEY environment variable not defined");

            string saAccountName = Environment.GetEnvironmentVariable("SA_ACCOUNT_NAME") ??
                throw new Exception("SA_ACCOUNT_NAME environment variable not defined");

            string saStorageUri = Environment.GetEnvironmentVariable("SA_STORAGE_URI") ??
                throw new Exception("SA_STORAGE_URI environment variable not defined");

            var serviceClient = new TableServiceClient(new Uri(saStorageUri),
            new TableSharedKeyCredential(saAccountName, saAccessKey));

            return serviceClient;
        }

        private void InsertPassInformation(Pass pass)
        {
            string saAccessKey = Environment.GetEnvironmentVariable("SA_ACCESS_KEY") ??
                throw new Exception("SA_ACCESS_KEY environment variable not defined");

            string saAccountName = Environment.GetEnvironmentVariable("SA_ACCOUNT_NAME") ??
                throw new Exception("SA_ACCOUNT_NAME environment variable not defined");

            string saStorageUri = Environment.GetEnvironmentVariable("SA_STORAGE_URI") ??
                throw new Exception("SA_STORAGE_URI environment variable not defined");

            var tableClient = new TableClient(new Uri(saStorageUri),
                "passes",
                new TableSharedKeyCredential(saAccountName, saAccessKey));

            tableClient.CreateIfNotExists();

            var passEntity = new TableEntity("prod", pass.serialNumber)
            {
                { "AccessKey", pass.authenticationToken },
                { "ExpirationDate", pass.expirationDate },
                { "RelevantDate", pass.relevantDate },
                { "EventName", pass.eventTicket.headerFields.First(hf => hf.key == "event").value },
                { "NameOnBadge", pass.eventTicket.primaryFields.First(hf => hf.key == "nameOnBadge").value },
                { "BarCode", pass.barcode.message },
                { "ForegroundColor", pass.foregroundColor },
                { "BackgroundColor", pass.backgroundColor },
                { "LabelColor", pass.labelColor }
            };
            tableClient.AddEntity(passEntity);
        }

        private string GetRGBColorTriplet(string hexCode)
        {
            if (hexCode.Length != 7) {
                throw new Exception("Expected a hex code with 7 characters");
            }
            string redHex = hexCode.Substring(1, 2);
            string greenHex = hexCode.Substring(3, 2);
            string blueHex = hexCode.Substring(5, 2);
            string result = "rgb(" +
            Convert.ToInt32(redHex, 16) + "," + Convert.ToInt32(greenHex, 16) + "," +
            Convert.ToInt32(blueHex, 16) +
            ")";
            return result;
        }
    }
}
