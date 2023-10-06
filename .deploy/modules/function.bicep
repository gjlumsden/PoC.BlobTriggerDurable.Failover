param namePrefix string = 'gl-aic-bcdr'
param location string
param storageAccountName string
param netFrameworkVersion string = 'v6.0'
param sku string = 'dynamic'
param skuCode string = 'Y1'
param workspaceId string

var locationAbbreviations = {
  uksouth: 'uks'
  ukwest: 'ukw'
}

var name = '${namePrefix}-func-${locationAbbreviations[location]}'
var hostingPlanName = '${namePrefix}-asp-${locationAbbreviations[location]}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource functionApp 'Microsoft.Web/sites@2018-11-01' = {
  name: name
  location: location
  tags: {
    'hidden-link: /app-insights-resource-id': appInsights.id
  }
  kind: 'functionapp'
  properties: {
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: uniqueString(name)
        }
      ]
      use32BitWorkerProcess: true
      ftpsState: 'FtpsOnly'
      netFrameworkVersion: netFrameworkVersion
    }
    clientAffinityEnabled: false
    httpsOnly: true
    serverFarmId: hostingPlan.id
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: hostingPlanName
  location: location
  tags: {}
  sku: {
    tier: sku
    name: skuCode
  }
  kind: 'functionapp'
  properties: {
  }
}

resource appInsights 'microsoft.insights/components@2020-02-02-preview' = {
  name: name
  kind: 'web'
  location: location
  tags: {}
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspaceId
  }
  dependsOn: []
}
