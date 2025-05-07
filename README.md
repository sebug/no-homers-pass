# No Homers Pass - Using the Apple Wallet for NFC Passes
Azure Static Web App (with Azure Functions and API Management) implementation of Apple Wallet.

Demoes the different steps needed to provide the full Apple Wallet experience.

You can see it in action [here](https://nice-field-03dde2c03.4.azurestaticapps.net).

In order to test the logging from the Apple Side, you can put in "Homer" as the first name, which will lead to a rejection when getting updated pass (because no Homers are allowed).

The Web Services for Apple Wallet require specific passes. Furthermore, Azure Functions inside Azure Static Web Apps filter out request
and response headers (quite hard to find any information about that). My solution was to put Azure API Management in front that rewrites
the URLs and headers. You can finde the definition in [apimanagement.bicep](apimanagement.bicep)

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

Then add an HTTP API and product. Rewrite the path URL to arguments to the function trigger and then deal with that.

Store the base URL of the API management in the environment variable API_MANAGEMENT_BASE_URL
