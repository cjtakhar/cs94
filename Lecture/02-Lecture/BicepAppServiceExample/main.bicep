/*
  This Bicep file deploys an Azure App Service in a specified resource group.
  It creates a resource group, an App Service Plan, and an App Service.

  Parameters:
  - location (string): The Azure region where the resources will be deployed.
  - resourceGroupName (string): The name of the resource group to be created.
  - appServicePlanName (string): The name of the App Service Plan to be created.
  - webAppName (string): The name of the App Service to be created.
  - planSkuName (string): The SKU name for the App Service Plan.
  - planSkuTier (string): The SKU tier for the App Service Plan.

  Usage:
  1. Set the desired values for the parameters.
  2. Deploy the Bicep file using the Azure CLI or PowerShell

  Azure CLI:
  az deployment sub create --name $("createStorageAccount_" + (Get-Date -Format "yyyyMMddHHMMss")) --location eastus --template-file main.bicep --parameters location=eastus resourceGroupName=rg_lecture02_bicep appServicePlanName=asp-linuxpaidbicep-01 webAppName=app-linuxdemobicep-cscie94-01 planSkuName=B1 planSkuTier=Basic
 
  PowerShell: 
  New-AzSubscriptionDeployment -Name ("createAspApp_" + (Get-Date -Format "yyyyMMddHHMMss")) -Location 'eastus' -TemplateFile 'main.bicep' -TemplateParameterObject @{location='eastus'; resourceGroupName='rg_lecture02_bicep'; appServicePlanName='asp-linuxpaidbicep-01'; webAppName='app-linuxdemobicep-cscie94-01'; planSkuName='B1'; planSkuTier='Basic'}

*/

/*
Documentation:
-------------
This Bicep template deploys an Azure resource group and a web app module.

Parameters:
- location: Defines the Azure region for resource deployment; defaults to 'eastus'.
- resourceGroupName: Specifies the name for the new resource group.
- appServicePlanName: Sets the name for the App Service Plan to be used by the web app.
- webAppName: Specifies the unique name for the web application.
- planSkuName: Indicates the SKU name for the App Service Plan (e.g., 'B1').
- planSkuTier: Specifies the pricing tier for the App Service Plan (e.g., 'Basic').

Target Scope:
- The deployment is scoped to a subscription.

Resources:
- A Resource Group is created using the provided name and location.
- An external module (appService.bicep) is invoked, deploying the web app and its associated App Service Plan within the created resource group.
*/
param location string = 'eastus'
param resourceGroupName string = 'rg_lecture02_appdemo_bicep'
param appServicePlanName string = 'asp-linuxpaidbicep'
param webAppName string = 'app-linuxdemobicep-cscie94'
param planSkuName string = 'B1'
param planSkuTier string = 'Basic'

/**
 * @file main.bicep
 * @description This Bicep file configures the deployment to be executed at the subscription level.
 *
 * The 'targetScope' directive specifies that all subsequent resource declarations within the file
 * will be deployed under the subscription context.
 */
targetScope = 'subscription'

/*
  Resource Group Declaration:
  - This section defines a resource group using the Microsoft.Resources/resourceGroups API (version 2021-04-01).
  - The resource group is configured with a dynamic name ('resourceGroupName') and location ('location').
  - Ensure that the parameters 'resourceGroupName' and 'location' are defined elsewhere in your Bicep template.
*/
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

// Module: App Service Deployment
//
// This module deploys an App Service by referencing the 'appService.bicep' file.
//
// Parameters:
//   - appServicePlanName: Name of the App Service Plan to be created or used.
//   - webAppName:         Name of the Web App resource.
//   - location:           Geographic region for deployment.
//   - planSkuName:        SKU name determining the performance characteristics of the service plan.
//   - planSkuTier:        SKU tier that further categorizes the service plan's pricing and performance.
//
// Scope:
//   The module is deployed at the Resource Group level.
//
// Note:
//   Ensure that all required parameters are provided in the parameter file or assigned in the current Bicep file.
module appService 'appService.bicep' = {
  name: 'appService2025-02-06'
  scope: resourceGroup
  params: {
    appServicePlanName: appServicePlanName
    webAppName: webAppName
    location: location
    planSkuName: planSkuName
    planSkuTier: planSkuTier
  }
}






