# No Homers Pass - Using the Apple Wallet for NFC Passes
We will do this as an Azure Functions app.

## Steps on the Azure Portal
You'll need to set up the Azure Static Web App. Also, you'll have to create a storage account.

Then go to that storage account and get one of the access keys (one day I'll set up managed identity, but this is not the project).

Store it in the environment variable SA_ACCESS_KEY on the static web app.

Store the account name in SA_ACCOUNT_NAME

Store the storage uri (like https://myaccount.table.core.windows.net ) in SA_STORAGE_URI

## Steps on the Apple Developer Portal Side

 - [Create a Pass Type Identifier](https://developer.apple.com/documentation/walletpasses/building-a-pass) - I saved the ID in the environment variable PASS_TYPE_ID for codespaces and actions and in the Azure Static Web App Environment variables
 - [Generate a Signing Certificate](https://developer.apple.com/documentation/walletpasses/building-a-pass#Generate-a-Signing-Certificate)

 After created the certificate, import it in keychain management and then export the private key as p12.

 Then create an X509Certificate2 from the bytes that we stored in the environment variable.

    var bytes = System.IO.File.ReadAllBytes("nohomers_dev_key.p12");
    var base64String = Convert.ToBase64String(bytes);

Note on generic implementation here: could do the processing client-side upon upload to avoid sending the file somewhere. But then again, that is what the password is for.

## Web Service for updates
Once the pass gets registered on your phone, apple will call a [register URL](https://developer.apple.com/documentation/walletpasses/adding-a-web-service-to-update-passes). Since this has a specific URL, we'll have to use Azure API Management to redirect that to our HTTP trigger.

Set up an Azure API Management, Consumption version.

