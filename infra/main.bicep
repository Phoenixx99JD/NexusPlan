// Infraestructura de NexusPlan en Azure:
//   - Azure SQL Server + Base de datos
//   - App Service Plan (Linux) + Web App .NET 8
//
// Despliegue:
//   az deployment group create -g <rg> -f infra/main.bicep \
//     -p appName=<nombre-unico> sqlAdminLogin=<usuario> sqlAdminPassword=<password>

@description('Prefijo/nombre único global para los recursos (solo minúsculas, números y guiones).')
@minLength(3)
@maxLength(40)
param appName string

@description('Región de Azure.')
param location string = resourceGroup().location

@description('Usuario administrador de Azure SQL.')
param sqlAdminLogin string

@description('Contraseña del administrador de Azure SQL.')
@secure()
param sqlAdminPassword string

@description('SKU del App Service Plan (F1 = gratuito, B1 = básico).')
@allowed([ 'F1', 'B1', 'B2', 'S1', 'P0v3' ])
param appServiceSku string = 'B1'

@description('SKU de la base de datos (Basic = 5 DTU).')
param sqlDatabaseSku string = 'Basic'

var sqlServerName = '${appName}-sql'
var sqlDatabaseName = 'NexusPlanDb'
var planName = '${appName}-plan'
var webAppName = appName

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// Permite que los servicios de Azure (App Service) accedan al servidor SQL.
resource allowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: sqlDatabaseSku
  }
}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  kind: 'linux'
  sku: {
    name: appServiceSku
  }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: appServiceSku != 'F1'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      healthCheckPath: '/health'
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
        { name: 'Database__ApplyMigrationsOnStartup', value: 'true' }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          type: 'SQLAzure'
          connectionString: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
      ]
    }
  }
  dependsOn: [ sqlDb ]
}

// Habilita las credenciales de publicación (necesarias para el publish profile de GitHub Actions).
resource scmBasicAuth 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-12-01' = {
  parent: webApp
  name: 'scm'
  properties: {
    allow: true
  }
}

output webAppName string = webApp.name
output apiUrl string = 'https://${webApp.properties.defaultHostName}'
output swaggerUrl string = 'https://${webApp.properties.defaultHostName}/swagger'
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
