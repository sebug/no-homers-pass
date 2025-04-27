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

                string privateKeyPassword = Environment.GetEnvironmentVariable("PRIVATE_KEY_PASSWORD") ??
                throw new Exception("PRIVATE_KEY_PASSWORD environment variable not defined");

                string privateKeyBase64 = Environment.GetEnvironmentVariable("PRIVATE_KEY") ??
                throw new Exception("PRIVATE_KEY environment variable not defined");
                var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);

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

                Directory.CreateDirectory(passDirectory);

                var pathsToHash = new List<string>();
                var pass = new Pass("No Homers", passTypeIdentifier, passDescription, serialNumber,
                teamIdentifier, expirationDate, relevantDate, eventTicket, barcode,
                foregroundColor: GetRGBColorTriplet(foregroundColorHex),
                backgroundColor: GetRGBColorTriplet(backgroundColorHex),
                labelColor: GetRGBColorTriplet(labelColorHex));
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
                        string hash = BitConverter.ToString(sha1.ComputeHash(fs))
                        .Replace("-", String.Empty).ToLower();
                        manifestDict[pathToHash] = hash;
                    }
                }

                await File.WriteAllTextAsync(Path.Combine(passDirectory, "manifest.json"),
                    JsonSerializer.Serialize(manifestDict));

                // Now sign the manifest
                var certificate = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(privateKeyBytes, privateKeyPassword);

                if (!certificate.HasPrivateKey)
                {
                    throw new Exception("Expected certificate to have a private key");
                }

                var rsaKey = certificate.GetRSAPrivateKey();
                if (rsaKey == null)
                {
                    throw new Exception("Expected private key to be RSA and present");
                }

                var manifestBytes = await File.ReadAllBytesAsync(Path.Combine(passDirectory, "manifest.json"));

                // See https://stackoverflow.com/questions/3916736/openssl-net-porting-a-ruby-example-to-c-sharp-from-railscasts-143-paypal-securi
                var cmsSigner = new CmsSigner(certificate);
                var content = new ContentInfo(manifestBytes);
                var signed = new SignedCms(content, true);
                signed.ComputeSignature(cmsSigner);

                var signedBytes = signed.Encode();

                File.WriteAllBytes(Path.Combine(passDirectory, "signature"), signedBytes);

                var memoryStream = new MemoryStream();
                ZipFile.CreateFromDirectory(passDirectory, memoryStream, CompressionLevel.Optimal, false); // in memory is fine, it's gonna be super small
                return new FileContentResult(memoryStream.ToArray(),
                "application/zip")
                {
                    FileDownloadName = "pass.pkpass"
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
