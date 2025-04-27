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
int formatVersion = 1)
{
    
}