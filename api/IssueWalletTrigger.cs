using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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
                string passDirectory = Path.Combine(temporaryDirectoryName, "NoHomersMembership.pass");
                var pass = new Pass("No Homers", "the.identifier");
                string passString = JsonSerializer.Serialize(pass);
                await File.WriteAllTextAsync(Path.Combine(passDirectory, "pass.json"),
                    passString);
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

    public record Pass(string organizationName,
    string passTypeIdentifier)
    {
        
    }
}
