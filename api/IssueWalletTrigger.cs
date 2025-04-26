using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sebug.Function.Models;
using System.Security.Cryptography;

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

                Directory.CreateDirectory(passDirectory);

                var pathsToHash = new List<string>();
                var pass = new Pass("No Homers", passTypeIdentifier, passDescription, serialNumber,
                teamIdentifier, expirationDate, relevantDate, eventTicket);
                string passString = JsonSerializer.Serialize(pass);
                await File.WriteAllTextAsync(Path.Combine(passDirectory, "pass.json"),
                    passString);

                pathsToHash.Add("pass.json");

                string currentDirectory = Directory.GetCurrentDirectory();

                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "icon.png"));
                pathsToHash.Add("icon.png");
                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "icon@2x.png"));
                pathsToHash.Add("icon@2x.png");
                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "icon@3x.png"));
                pathsToHash.Add("icon@3x.png");
                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "logo.png"));
                pathsToHash.Add("logo.png");
                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "logo@2x.png"));
                pathsToHash.Add("logo@2x.png");
                File.Copy(Path.Combine(currentDirectory, "logo_full.png"),
                Path.Combine(passDirectory, "logo@3x.png"));
                pathsToHash.Add("logo@3x.png");

                var manifestDict = new Dictionary<string, string>();
                foreach (var pathToHash in pathsToHash)
                {
                    using (var sha1 = SHA1.Create())
                    using (var fs = File.OpenRead(Path.Combine(passDirectory, pathToHash)))
                    {
                        string hash = BitConverter.ToString(sha1.ComputeHash(fs));
                        manifestDict[pathToHash] = hash;
                    }
                }

                await File.WriteAllTextAsync(Path.Combine(passDirectory, "manifest.json"),
                    JsonSerializer.Serialize(manifestDict));

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
