namespace Sebug.Function.Models;

public record Pass(string organizationName,
string passTypeIdentifier,
int formatVersion = 1,
string description = "E-TICKET",
string passTypeIdentifier = "ch.sebug.passes.ticket")
{
    
}