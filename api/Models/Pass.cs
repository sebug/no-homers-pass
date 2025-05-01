namespace Sebug.Function.Models;

public record Pass(string organizationName,
string passTypeIdentifier,
string description,
string serialNumber,
string teamIdentifier,
string expirationDate,
string relevantDate,
EventTicket eventTicket,
Barcode barcode,
string foregroundColor,
string backgroundColor,
string labelColor,
string authenticationToken,
string webServiceURL,
int formatVersion = 1)
{
    
}