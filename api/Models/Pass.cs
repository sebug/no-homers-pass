namespace Sebug.Function.Models;

public record Pass(string organizationName,
string passTypeIdentifier,
string description,
string serialNumber,
string teamIdentifier,
string expirationDate,
string relevantDate,
EventTicket eventTicket,
int formatVersion = 1)
{
    
}