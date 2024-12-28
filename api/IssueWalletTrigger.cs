using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sebug.Function.Models;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string firstName = req.Form["first_name"].FirstOrDefault() ?? String.Empty;
            string? temporaryDirectoryName = null;
            try
            {
                temporaryDirectoryName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(temporaryDirectoryName);
                string passDirectory = Path.Combine(temporaryDirectoryName, "No Homers Membership.pass");

                string passTypeIdentifier = Environment.GetEnvironmentVariable("PASS_TYPE_ID") ??
                    throw new Exception("PASS_TYPE_ID environment variable not defined");

                Directory.CreateDirectory(passDirectory);
                var pass = new Pass("No Homers", passTypeIdentifier);
                string passString = JsonSerializer.Serialize(pass);
                await File.WriteAllTextAsync(Path.Combine(passDirectory, "pass.json"),
                    passString);

                var assy = typeof(IssueWalletTrigger).Assembly;
                var logoStream = assy.GetManifestResourceStream("logo_full.png");

                if (logoStream == null)
                {
                    throw new Exception("Did not find included file logo_full.png");
                }

                using (var logoFs = File.Create(Path.Combine(passDirectory, "logo_full.png")))
                {
                    await logoStream.CopyToAsync(logoFs);
                }

                var memoryStream = new MemoryStream();
                ZipFile.CreateFromDirectory(temporaryDirectoryName, memoryStream); // in memory is fine, it's gonna be super small
                return new FileContentResult(memoryStream.ToArray(),
                "application/zip")
                {
                    FileDownloadName = "pass.zip"
                };
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            finally
            {
                if (!String.IsNullOrEmpty(temporaryDirectoryName) && Directory.Exists(temporaryDirectoryName))
                {
                    Directory.Delete(temporaryDirectoryName, true);
                }
            }
        }
    }
}
