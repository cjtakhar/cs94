/*
This Bicep module creates an Azure Storage Account with configurable parameters.

Parameters:
- storageAccountName (string): The name of the storage account to be created.
- skuName (string, optional): The SKU name of the storage account. Default is 'Standard_LRS'.
- location (string, optional): The location/region where the storage account will be created. Default is 'eastus'.
- accessTier (string, optional): The access tier for the storage account. Default is 'Hot'.
- allowPublicAccess (bool, optional): Specifies whether public access to the storage account is allowed. Default is false.
- networkDefaultAction (string, optional): The default action for network access. Default is 'Allow'.

Resources:
- storageAccount: Creates a storage account with the specified parameters.

Outputs:
- storageAccountId (string): The resource ID of the created storage account.
*/
param storageAccountName string
param skuName string = 'Standard_LRS'
param location string = 'eastus'
param accessTier string = 'Hot'
param allowPublicAccess bool = false
param networkDefaultAction string = 'Allow'

// Resource block to create a storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: skuName
  }
  kind: 'StorageV2'
  properties: {
    accessTier: accessTier
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: allowPublicAccess
    networkAcls: {
      defaultAction: networkDefaultAction
    }
  }
}

// Output to retrieve the ID of the storage account
output storageAccountId string = storageAccount.id
