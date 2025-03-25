In order to run this sample locally you will need to add a file called: local.settings.json with the 
following content:

{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "< your connection string>",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
    }
}

Also create the following containers in your Blob Containers
convertedimages
failedimages
imagestoconverttograyscale

A table will be created for you called:
jobs


The convertedimages container will contain images that were converted with success
The failedimages container will contain images(files) that were not able to be converted
The imagestoconverttograyscale folder is where you upload images to

The jobs table contains the status and results of the job
A job represents converting an image uploaded in the imagestoconverttograyscale container into gray scale
