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
        var response = tableClient.GetEntity<TableEntity>("prod", serialNumber);
        if (response == null)
        {
            return null;
        }
        return response.Value;
    }
}