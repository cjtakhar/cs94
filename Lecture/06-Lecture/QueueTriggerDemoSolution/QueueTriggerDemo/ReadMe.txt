In order to run this sample locally you will need to add a file called: local.settings.json with the following content:
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "< your connection string>",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
    }
}

Also create the following queue
myqueue-items

Add json messages to queue see them output in the output log ex
{ "message" : "Hello World!"}