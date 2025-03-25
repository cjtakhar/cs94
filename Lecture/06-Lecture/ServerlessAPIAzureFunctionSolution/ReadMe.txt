In order to run this sample locally you will need to add a file called: local.settings.json with the following content:
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "< your connection string>",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet"
    }
}

