param namePrefix string
param location string

var locationAbbreviations = {
  uksouth: 'uks'
  ukwest: 'ukw'
}
var storageAccountName = '${namePrefix}${locationAbbreviations[location]}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_RAGRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowCrossTenantReplication: false
    allowSharedKeyAccess: true
    minimumTlsVersion: 'TLS1_2'
    routingPreference: {
      routingChoice: 'MicrosoftRouting'
    }
    supportsHttpsTrafficOnly: true
  }
  kind: 'StorageV2'
}

output storageAccountName string = storageAccount.name
