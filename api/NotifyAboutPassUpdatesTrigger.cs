using System.Text;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sebug.Function.Models;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
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

            var settings = PassSettings.GetFromEnvironment();

            var certificate = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(settings.PrivateKeyBytes,
            settings.PrivateKeyPassword);

            var clientHandler = new HttpClientHandler();
            clientHandler.ClientCertificates.Add(certificate);
            clientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            using (var client = new HttpClient(clientHandler))
            {
                client.BaseAddress = new Uri(settings.APNSUrl);
                var payload = new StringContent("{}", Encoding.UTF8, "application/json"); // empty object as per spec
                payload.Headers.Add(":method", "POST");
                payload.Headers.Add(":path", "/3/device/" + deviceLibraryIdentifier);
                payload.Headers.Add("apns-push-type", "alert");
                payload.Headers.Add("apns-topic", settings.PassTypeIdentifier);

                var response = await client.PostAsync("/3/device/" + deviceLibraryIdentifier, payload);
                string responseString = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return new BadRequestObjectResult(responseString);
                }
                return new OkObjectResult(responseString);
            }
        }
    }
}
