/*
  This Bicep file deploys an Azure App Service Plan and a Web App.
  It takes the following parameters:
  - appServicePlanName: The name of the App Service Plan.
  - webAppName: The name of the Web App.
  - location: The Azure region where the resources will be deployed.
  - planSkuName: The SKU name for the App Service Plan (default: 'B1').
  - planSkuTier: The SKU tier for the App Service Plan (default: 'Basic').

  The file defines two resources:
  - appServicePlan: An Azure App Service Plan resource with the specified name, location, and SKU.
    It is configured for Linux and has the specified SKU name and tier.
  - webApp: An Azure Web App resource with the specified name, location, and server farm ID.
    It is configured to only allow HTTPS connections.

  Example usage:
  ```
  bicep build appService.bicep -p appServicePlanName=myPlan -p webAppName=myWebApp -p location=eastus
  ```
*/

param appServicePlanName string
param webAppName string
param location string
param planSkuName string = 'B1'
param planSkuTier string = 'Basic'

// This section defines an App Service Plan resource for an Azure web application.
// It uses the 'Microsoft.Web/serverfarms' resource at API version 2021-01-01.
// The 'appServicePlanName' and 'location' parameters specify the name and the Azure region in which the plan is deployed.
// The 'reserved' property is set to true to indicate that the plan is configured for Linux hosting.
// The SKU configuration (using 'planSkuName' and 'planSkuTier') determines the performance and pricing tier of the plan.
resource appServicePlan 'Microsoft.Web/serverfarms@2021-01-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: true // This is for Linux
  }
  sku: {
    name: planSkuName
    tier: planSkuTier
  }
}

// This resource definition creates an Azure Web App (Microsoft.Web/sites) using API version 2021-01-01.
// 
// Parameters and Configuration:
//   - webAppName: Specifies the name of the web application.
//   - location: Determines the Azure region in which the web application is deployed.
// 
// Resource Properties:
//   - serverFarmId: Links the web app to an App Service Plan (defined by appServicePlan) that provides the hosting environment.
//   - httpsOnly: Enforces secure (HTTPS) connections to the web application.
resource webApp 'Microsoft.Web/sites@2021-01-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
  }
}
