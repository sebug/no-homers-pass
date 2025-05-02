using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Sebug.Function.Models;

namespace Sebug.Function;

public record PkPassFileGenerator(Pass Pass)
{
    public async Task<byte[]> Generate(byte[] privateKeyBytes, string privateKeyPassword)
    {
        byte[] passContentBytes = new byte[0];
        string? temporaryDirectoryName = null;
        try
        {
            temporaryDirectoryName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(temporaryDirectoryName);
            string passDirectory = Path.Combine(temporaryDirectoryName, "No Homers Membership.pass");

            var pathsToHash = new List<string>();
            Directory.CreateDirectory(passDirectory);

            string passString = JsonSerializer.Serialize(Pass);
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

            var manifestBytes = await File.ReadAllBytesAsync(Path.Combine(passDirectory, "manifest.json"));

            // See https://stackoverflow.com/questions/3916736/openssl-net-porting-a-ruby-example-to-c-sharp-from-railscasts-143-paypal-securi
            var cmsSigner = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
            cmsSigner.IncludeOption = X509IncludeOption.EndCertOnly;
            cmsSigner.SignedAttributes.Add(new Pkcs9SigningTime(DateTime.UtcNow));
            var content = new ContentInfo(manifestBytes);
            var signed = new SignedCms(content, true);
            signed.ComputeSignature(cmsSigner);

            var signedBytes = signed.Encode();

            File.WriteAllBytes(Path.Combine(passDirectory, "signature"), signedBytes);

            var memoryStream = new MemoryStream();
            ZipFile.CreateFromDirectory(passDirectory, memoryStream, CompressionLevel.Optimal, false); // in memory is fine, it's gonna be super small
            passContentBytes = memoryStream.ToArray();
        }
        finally 
        {
            if (!String.IsNullOrEmpty(temporaryDirectoryName) && Directory.Exists(temporaryDirectoryName))
            {
                Directory.Delete(temporaryDirectoryName, true);
            }
        }

        return passContentBytes;
    }
}