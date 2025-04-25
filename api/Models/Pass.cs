namespace Sebug.Function.Models;

public record Pass(string organizationName,
string passTypeIdentifier,
string description,
string serialNumber,
int formatVersion = 1)
{
    
}