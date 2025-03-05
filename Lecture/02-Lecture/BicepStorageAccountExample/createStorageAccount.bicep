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
az deployment group create --name $("createStorageAccount_" + (Get-Date -Format "yyyyMMddHHMMss")) --resource-group rg_02_biceplecture  --template-file createStorageAccount.bicep --parameters storageAccountName='stbicepdemobicep' 
 
Powershell:
New-AzResourceGroupDeployment -Name ("createStorageAccount_" + (Get-Date -Format "yyyyMMddHHMMss")) -ResourceGroupName "rg_02_biceplecture" -TemplateFile "createStorageAccount.bicep" -TemplateParameterObject @{storageAccountName = 'stbicepdemobicep'}
*/

// Parameter to specify the name of the storage account
@description('The name of the storage account to create')
param storageAccountName string

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

@description('Indicates the default network action. Default is Allow')
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

// Output to retrieve the ID of the storage account
output storageAccountId string = storageAccount.id
