using Azure.Data.Tables;
using Sebug.Function.Models;

namespace Sebug.Function;

public record PassStorageProvider(string StorageAccountAccessKey,
    string StorageAccountName,
    string StorageAccountUri)
{
    public static PassStorageProvider GetFromEnvironment()
    {
        string saAccessKey = Environment.GetEnvironmentVariable("SA_ACCESS_KEY") ??
            throw new Exception("SA_ACCESS_KEY environment variable not defined");

        string saAccountName = Environment.GetEnvironmentVariable("SA_ACCOUNT_NAME") ??
            throw new Exception("SA_ACCOUNT_NAME environment variable not defined");

        string saStorageUri = Environment.GetEnvironmentVariable("SA_STORAGE_URI") ??
            throw new Exception("SA_STORAGE_URI environment variable not defined");

        return new PassStorageProvider(saAccessKey, saAccountName, saStorageUri);
    }

    public TableClient GetTableClient(string tableName)
    {
        var tableClient = new TableClient(new Uri(StorageAccountUri),
        tableName,
        new TableSharedKeyCredential(StorageAccountName, StorageAccountAccessKey));
        return tableClient;
    }

    public TableEntity? GetPassBySerialNumber(string serialNumber)
    {
        var tableClient = GetTableClient("passes");
        var response = tableClient.GetEntityIfExists<TableEntity>("prod", serialNumber);
        if (response == null)
        {
            return null;
        }
        return response.Value;
    }

    public Pass MapTableEntityToPass(TableEntity entity)
    {
        var settings = PassSettings.GetFromEnvironment();
        var eventTicket = new EventTicket(headerFields: new List<Field> { new Field (
                key : "event",
                label : "Event",
                value : entity["EventName"]?.ToString() ?? String.Empty
            )
            },
            primaryFields: new List<Field> { new Field (
                key : "nameOnBadge",
                label : "Name",
                value : entity["NameOnBadge"]?.ToString() ?? String.Empty
            )
            },
            secondaryFields: [],
            auxiliaryFields: [],
            backFields: new List<Field> {
                new Field(
                key : "fullName",
                label : "Full Name",
                value : entity["NameOnBadge"]?.ToString() ?? String.Empty
                )
        });

        var barcode = new Barcode(entity.RowKey.Replace("-", ""),
        "PKBarcodeFormatQR", entity.RowKey.Replace("-", ""), "utf-8");
        var pass = new Pass("No Homers", settings.PassTypeIdentifier, settings.PassDescription, entity.RowKey,
        settings.TeamIdentifier, entity["ExpirationDate"]?.ToString() ?? String.Empty, entity["RelevantDate"]?.ToString() ?? String.Empty,
        eventTicket, barcode,
        foregroundColor: entity["ForegroundColor"]?.ToString() ?? String.Empty,
        backgroundColor: entity["BackgroundColor"]?.ToString() ?? String.Empty,
        labelColor: entity["LabelColor"]?.ToString() ?? String.Empty,
        authenticationToken: entity["AccessKey"]?.ToString() ?? String.Empty,
        webServiceURL: settings.APIManagementBaseURL);

        return pass;
    }
}