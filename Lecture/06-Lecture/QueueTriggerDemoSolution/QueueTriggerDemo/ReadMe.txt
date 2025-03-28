In order to run this sample locally you will need to add a file called: local.settings.json with the following content:
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "StorageConnections:blobServiceUri": "https://<your storage account>.blob.core.windows.net/",
    "StorageConnections:queueServiceUri": "https://<your storage account>.queue.core.windows.net/",
    "StorageConnections:tableServiceUri": "https://<your storage account>.table.core.windows.net/"
  }
}

Remember when adding the StoageConnections values to replace <your storage account> with the name of your storage account.
Also when deploying to Azure remember to set them in the Azure portal as well.
You will need to name them as follows:
StorageConnections__blobServiceUri
StorageConnections__queueServiceUri
StorageConnections__tableServiceUri

NOTE: When running in Azure you will still need to set the AzureWebJobsStorage value to a 
      connection string for your storage account.
	  This is because the Azure Functions runtime uses it to manage the function host and other resources.
	  It is possible to configure the function to use managed identities fully,
	  but this is not covered in this sample and requires additional deployment steps and configuration.

Also don't forget to set the AZURE_CLIENT_ID using the client Id from the
User Assigned Managed Identity you created for and assigned to the function app.

Also create the following queue
myqueue-items-function
myjsonqueue-items-function

Add json messages to queue see them output in the output log ex
{ "message" : "Hello World!"}

Add text messages to the myqueue-items-function
"Hello World!"

If you add a message with the word exception it will throw an exception and the message will be moved to the poison queue.
The poison queue is called myqueue-items-function-poison and the message will be output to the log with the following message:
"PoisonHandler: C# Queue trigger function processed message: {message}"

It will also create a blob in the poisonmessages container.
The blob name will be the message id and the content will be the message.