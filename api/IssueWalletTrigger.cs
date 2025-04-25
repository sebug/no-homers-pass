using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            FunctionContext context)
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

                string passDescription = Environment.GetEnvironmentVariable("PASS_DESCRIPTION") ??
                throw new Exception("PASS_DESCRIPTION environment variable not defined");

                string serialNumber = Guid.NewGuid().ToString().ToUpper();

                string teamIdentifier = Environment.GetEnvironmentVariable("TEAM_IDENTIFIER") ??
                throw new Exception("TEAM_IDENTIFIER environment variable not defined");

                var expiration = DateTimeOffset.Now.AddDays(1);
                expiration = new DateTimeOffset(expiration.Year, expiration.Month, expiration.Day, expiration.Hour,
                expiration.Minute, expiration.Second, TimeSpan.FromHours(2));

                string expirationDate = expiration.ToString("yyyy-MM-ddTHH:mm:sszzz");

                var relevant = DateTimeOffset.Now;
                
                string relevantDate = new DateTimeOffset(relevant.Year, relevant.Month, relevant.Day,
                relevant.Hour, relevant.Minute, relevant.Second, TimeSpan.FromHours(2)).ToString("yyyy-MM-ddTHH:mm:sszzz");

                Directory.CreateDirectory(passDirectory);
                var pass = new Pass("No Homers", passTypeIdentifier, passDescription, serialNumber,
                teamIdentifier, expirationDate, relevantDate);
                string passString = JsonSerializer.Serialize(pass);
                await File.WriteAllTextAsync(Path.Combine(passDirectory, "pass.json"),
                    passString);

                string currentDirectory = Directory.GetCurrentDirectory();

                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "icon.png"));
                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "icon@2x.png"));
                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "icon@3x.png"));
                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "logo.png"));
                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "logo@2x.png"));
                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "logo@3x.png"));

                var memoryStream = new MemoryStream();
                ZipFile.CreateFromDirectory(passDirectory, memoryStream, CompressionLevel.Optimal, false); // in memory is fine, it's gonna be super small
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
