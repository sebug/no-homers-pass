namespace Sebug.Function.Models;

public record Pass(string organizationName,
string passTypeIdentifier,
string description,
string serialNumber,
string teamIdentifier,
string expirationDate,
int formatVersion = 1)
{
    
}