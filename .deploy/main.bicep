targetScope = 'subscription'
@description('For this example, we will create a resource group in primary region for all resources.')
param appNamePrefix string
param resourceGroupName string
param primaryLocation string
param secondaryLocation string
param workspaceName string
param workspaceResourceGroup string

var locations = [primaryLocation, secondaryLocation]
var storageAccountNamePrefix = '${toLower(replace(appNamePrefix, '-', ''))}stg'

resource workspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: workspaceName
  scope: resourceGroup(workspaceResourceGroup)
}

module functionApps 'modules/function.bicep' = [for (location, i) in locations: {
  name: '${deployment().name}-func-${i}-${location}'
  scope: resourceGroup(resourceGroupName)
  params: {
    location: location
    storageAccountName: storage.outputs.storageAccountName
    workspaceId: workspace.id
    namePrefix: appNamePrefix
  }
}]

module storage 'modules/storage.bicep' = {
  name: '${deployment().name}-stg-${primaryLocation}'
  scope: resourceGroup(resourceGroupName)
  params: {
    location: primaryLocation
    namePrefix: storageAccountNamePrefix
  }
}
