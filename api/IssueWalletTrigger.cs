using System.IO.Compression;
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
                await File.WriteAllTextAsync(Path.Combine(temporaryDirectoryName, "message.txt"),
                    "Walletino for " + firstName);
                var memoryStream = new MemoryStream();
                ZipFile.CreateFromDirectory(temporaryDirectoryName, memoryStream); // in memory is fine, it's gonna be super small
                return new FileContentResult(memoryStream.ToArray(),
                "application/zip")
                {
                    FileDownloadName = "pass.zip"
                };
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
