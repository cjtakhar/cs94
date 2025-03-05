/*
 This Bicep file deploys an azure storage account. The resource group is specified in the 
 Azure CLI Command line or PowerShell Command Line

 Parameters:
 - storageAccountName (string): The name of the storage account to create
 - location (string): The name of the Azure region where the resources will be deployed, defaults to the region
                      of the resource group is located in.
 - skuName (string): The SKU name for the storage account defaults to Standard_LRS
 - accessTier (string): The access tier for the storage account defaults to Hot
 - networkDefaultAction (string) Specifying if public access is allowed, default is false
 - allowPublicAccess (string) Indicates the default network action. Default is Allow

Usage:
1. Set the desired values for the parameters
2. Deploy the Bicep file using the Azure CLI or PowerShell

 Examples must be all on the same line broken out here for clarity
 
Azure CLI:
az deployment group create --name $("createStorageAccount_" + (Get-Date -Format "yyyyMMddHHMMss")) --resource-group rg_02_biceplecture  --template-file createStorageAccountEx.bicep --parameters storageAccountName='stbicepdemobicep'
 
Powershell:
New-AzResourceGroupDeployment -Name ("createStorageAccount_" + (Get-Date -Format "yyyyMMddHHMMss")) -ResourceGroupName "rg_02_biceplecture" -TemplateFile "createStorageAccountEx.bicep" -TemplateParameterObject @{ storageAccountName = 'stbicepdemobicep'; containerName = 'mycontainer'; queueName = 'myqueue' }
*/

// Parameter to specify the name of the storage account
@description('The name of the storage account to create')
param storageAccountName string

// The container name to be created
@description('The container name to be created')
param containerName string

// The queue name to be created
@description('The queue name to be created')
param queueName string

// Parameter to specify the location of the storage account,
// defaulted from resource group's location
@description('The location of the storage group, defaults to the resource group location.')
param location string = resourceGroup().location

// Parameter to specify the SKU name of the storage account
@description('The the SKU defaults to Standard_LRS')
param skuName string = 'Standard_LRS' // Standard Locally-Redundant Storage

// Parameter to specify the access tier of the storage account
@description('The access tier of the storage account, defaults to Hot')
param accessTier string = 'Hot'

// Parameter to specify if public access is allowed
@description('Specifying if public access is allowed, default is false')
param allowPublicAccess bool = false

@description('Indicates the default network action. Default is Allow ')
@allowed(['Allow','Deny'])
param networkDefaultAction string ='Allow'

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

/*
  Creates a blob service in a storage account.

  This resource creates a blob service within a specified storage account. The blob service allows you to store and retrieve large amounts of unstructured data, such as text or binary files.

  Parameters:
    - parent: The parent resource to associate the blob service with.
    - name: The name of the blob service.
    - properties: Additional properties for the blob service.

*/
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2021-08-01' = {
  parent: storageAccount
  name: 'default'
  properties: {}
}

/*
Creates a storage container within a blob service in a storage account.

This resource block defines a storage container using the 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-08-01' resource type. It specifies the parent blob service and the name of the container.

Parameters:
- parent: The parent blob service resource to which the container belongs.
- name: The name of the container.
*/
resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-08-01' = {
  parent: blobService
  name: containerName
}

/*
Creates a queue service for the storage account.

This resource block creates a queue service for the specified storage account. The queue service allows you to store and retrieve messages in a queue.

Parameters:
- parent: Specifies the parent resource to associate the queue service with. In this case, it is the storage account.
- name: Specifies the name of the queue service. In this example, the name is set to 'default'.
- properties: Specifies additional properties for the queue service. Currently, no additional properties are provided.

Returns:
- A queue service resource that is associated with the specified storage account.

*/
resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2021-08-01' = {
  parent: storageAccount
  name: 'default'
  properties: {}
}

/*
Creates a storage queue in a storage account.

This resource block creates a storage queue in a storage account using the 'Microsoft.Storage/storageAccounts/queueServices/queues' resource type.
The 'parent' property specifies the parent resource, which in this case is the 'queueService' resource.
The 'name' property specifies the name of the queue.
*/
resource queue 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-08-01' = {
  parent: queueService
  name: queueName
}

output storageAccountId string = storageAccount.id
output containerId string = container.id
output queueId string = queue.id
