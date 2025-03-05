# Configuration Notes

## Locally
To run the application locally, you will need to add the following configuration to your `secrets.json` file:
```json
{
"APPLICATIONINSIGHTS_CONNECTION_STRING": "Your app insights connection string"
"ApplicationInsights":"AuthenticationApiKey": "Your Authenticate SDK control channel api key",
}
```
## In Azure Linux App Service
To run the application in Azure, you will need to add the following configuration to your `Application Settings`:
```json
{
"APPLICATIONINSIGHTS_CONNECTION_STRING"  "Your app insights connection string"
"ApplicationInsights__AuthenticationApiKey"  "Your Authenticate SDK control channel api key",
}
```

## In Azure Windows App Service
To run the application in Azure, you will need to add the following configuration to your `Application Settings`:
```json
{
"APPLICATIONINSIGHTS_CONNECTION_STRING"  "Your app insights connection string"
"ApplicationInsights:AuthenticationApiKey"  "Your Authenticate SDK control channel api key",
}
```