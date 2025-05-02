namespace Sebug.Function.Models;

public record PassSettings(string PassTypeIdentifier,
    string PassDescription,
    string TeamIdentifier,
    string APIManagementBaseURL,
    string PrivateKeyPassword,
    byte[] PrivateKeyBytes)
{
    public static PassSettings GetFromEnvironment()
    {
        string passTypeIdentifier = Environment.GetEnvironmentVariable("PASS_TYPE_ID") ??
            throw new Exception("PASS_TYPE_ID environment variable not defined");

        string passDescription = Environment.GetEnvironmentVariable("PASS_DESCRIPTION") ??
        throw new Exception("PASS_DESCRIPTION environment variable not defined");

        string teamIdentifier = Environment.GetEnvironmentVariable("TEAM_IDENTIFIER") ??
        throw new Exception("TEAM_IDENTIFIER environment variable not defined");

        string apiManagementBaseURL = Environment.GetEnvironmentVariable("API_MANAGEMENT_BASE_URL") ??
        throw new Exception("API_MANAGEMENT_BASE_URL environment variable not defined");

        string privateKeyPassword = Environment.GetEnvironmentVariable("PRIVATE_KEY_PASSWORD") ??
        throw new Exception("PRIVATE_KEY_PASSWORD environment variable not defined");

        string privateKeyBase64 = Environment.GetEnvironmentVariable("PRIVATE_KEY") ??
        throw new Exception("PRIVATE_KEY environment variable not defined");
        var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);

        return new PassSettings(PassTypeIdentifier: passTypeIdentifier,
        PassDescription: passDescription,
        TeamIdentifier: teamIdentifier,
        APIManagementBaseURL: apiManagementBaseURL,
        PrivateKeyPassword: privateKeyPassword,
        PrivateKeyBytes: privateKeyBytes);
    }
}