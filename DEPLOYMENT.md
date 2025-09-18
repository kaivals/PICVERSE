# PicVerse Deployment Guide

## Azure Deployment Architecture

### Overview
This guide covers deploying PicVerse to Microsoft Azure using best practices for scalability, security, and performance.

### Azure Services Used

#### Backend Services
- **Azure App Service** - Host ASP.NET Core Web API
- **Azure SQL Database** - Primary database
- **Azure SignalR Service** - Real-time messaging
- **Azure Blob Storage** - Media file storage
- **Azure Key Vault** - Secrets management
- **Azure Application Insights** - Monitoring and logging
- **Azure CDN** - Content delivery network

#### Frontend Services
- **Azure Static Web Apps** - Host Angular application
- **Azure CDN** - Global content delivery

### Prerequisites

1. **Azure Subscription** - Active Azure subscription
2. **Azure CLI** - Install Azure CLI
3. **.NET 9 SDK** - For local development
4. **Node.js 18+** - For Angular application
5. **Visual Studio 2022** or **VS Code**

### Step 1: Create Azure Resources

#### 1.1 Resource Group
```bash
az group create --name rg-picverse-prod --location "East US"
```

#### 1.2 Azure SQL Database
```bash
# Create SQL Server
az sql server create \
  --name sql-picverse-prod \
  --resource-group rg-picverse-prod \
  --location "East US" \
  --admin-user picverseadmin \
  --admin-password "YourSecurePassword123!"

# Create Database
az sql db create \
  --resource-group rg-picverse-prod \
  --server sql-picverse-prod \
  --name picverse-db \
  --service-objective S1
```

#### 1.3 Azure App Service
```bash
# Create App Service Plan
az appservice plan create \
  --name asp-picverse-prod \
  --resource-group rg-picverse-prod \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group rg-picverse-prod \
  --plan asp-picverse-prod \
  --name app-picverse-api-prod \
  --runtime "DOTNETCORE:9.0"
```

#### 1.4 Azure Blob Storage
```bash
# Create Storage Account
az storage account create \
  --name stpicverseprod \
  --resource-group rg-picverse-prod \
  --location "East US" \
  --sku Standard_LRS

# Create Container for media files
az storage container create \
  --name media \
  --account-name stpicverseprod \
  --public-access blob
```

#### 1.5 Azure SignalR Service
```bash
az signalr create \
  --name signalr-picverse-prod \
  --resource-group rg-picverse-prod \
  --sku Standard_S1 \
  --service-mode Default
```

#### 1.6 Azure Key Vault
```bash
az keyvault create \
  --name kv-picverse-prod \
  --resource-group rg-picverse-prod \
  --location "East US"
```

#### 1.7 Application Insights
```bash
az monitor app-insights component create \
  --app ai-picverse-prod \
  --location "East US" \
  --resource-group rg-picverse-prod
```

### Step 2: Configure Application Settings

#### 2.1 Backend Configuration
Update `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "@Microsoft.KeyVault(SecretUri=https://kv-picverse-prod.vault.azure.net/secrets/DatabaseConnectionString/)",
    "SignalRConnection": "@Microsoft.KeyVault(SecretUri=https://kv-picverse-prod.vault.azure.net/secrets/SignalRConnectionString/)"
  },
  "JwtSettings": {
    "SecretKey": "@Microsoft.KeyVault(SecretUri=https://kv-picverse-prod.vault.azure.net/secrets/JwtSecretKey/)",
    "Issuer": "https://app-picverse-api-prod.azurewebsites.net",
    "Audience": "https://picverse-frontend.azurestaticapps.net",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryInDays": 7
  },
  "BlobStorage": {
    "ConnectionString": "@Microsoft.KeyVault(SecretUri=https://kv-picverse-prod.vault.azure.net/secrets/BlobStorageConnectionString/)",
    "ContainerName": "media"
  },
  "ApplicationInsights": {
    "ConnectionString": "@Microsoft.KeyVault(SecretUri=https://kv-picverse-prod.vault.azure.net/secrets/ApplicationInsightsConnectionString/)"
  }
}
```

#### 2.2 Store Secrets in Key Vault
```bash
# Database Connection String
az keyvault secret set \
  --vault-name kv-picverse-prod \
  --name DatabaseConnectionString \
  --value "Server=tcp:sql-picverse-prod.database.windows.net,1433;Database=picverse-db;User ID=picverseadmin;Password=YourSecurePassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# JWT Secret Key
az keyvault secret set \
  --vault-name kv-picverse-prod \
  --name JwtSecretKey \
  --value "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"

# SignalR Connection String
SIGNALR_CONNECTION=$(az signalr key list --name signalr-picverse-prod --resource-group rg-picverse-prod --query primaryConnectionString -o tsv)
az keyvault secret set \
  --vault-name kv-picverse-prod \
  --name SignalRConnectionString \
  --value "$SIGNALR_CONNECTION"

# Blob Storage Connection String
STORAGE_CONNECTION=$(az storage account show-connection-string --name stpicverseprod --resource-group rg-picverse-prod --query connectionString -o tsv)
az keyvault secret set \
  --vault-name kv-picverse-prod \
  --name BlobStorageConnectionString \
  --value "$STORAGE_CONNECTION"
```

### Step 3: Deploy Backend API

#### 3.1 Configure App Service
```bash
# Enable Key Vault integration
az webapp identity assign \
  --name app-picverse-api-prod \
  --resource-group rg-picverse-prod

# Get the principal ID
PRINCIPAL_ID=$(az webapp identity show --name app-picverse-api-prod --resource-group rg-picverse-prod --query principalId -o tsv)

# Grant Key Vault access
az keyvault set-policy \
  --name kv-picverse-prod \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

#### 3.2 Deploy using Azure CLI
```bash
# Build and publish
dotnet publish Backend/PicVerse.API/PicVerse.API.csproj -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to App Service
az webapp deployment source config-zip \
  --resource-group rg-picverse-prod \
  --name app-picverse-api-prod \
  --src deploy.zip
```

#### 3.3 Configure CORS
```bash
az webapp cors add \
  --resource-group rg-picverse-prod \
  --name app-picverse-api-prod \
  --allowed-origins https://picverse-frontend.azurestaticapps.net
```

### Step 4: Deploy Frontend

#### 4.1 Build Angular Application
```bash
cd Frontend
npm install
npm run build --prod
```

#### 4.2 Deploy to Azure Static Web Apps
```bash
# Install Static Web Apps CLI
npm install -g @azure/static-web-apps-cli

# Deploy
swa deploy ./dist/picverse-frontend \
  --app-name picverse-frontend \
  --resource-group rg-picverse-prod
```

### Step 5: Database Migration

#### 5.1 Run Entity Framework Migrations
```bash
# Update connection string in appsettings.json temporarily
dotnet ef database update --project Backend/PicVerse.Infrastructure --startup-project Backend/PicVerse.API
```

### Step 6: Configure Custom Domain (Optional)

#### 6.1 Backend API Domain
```bash
# Add custom domain to App Service
az webapp config hostname add \
  --webapp-name app-picverse-api-prod \
  --resource-group rg-picverse-prod \
  --hostname api.picverse.com

# Enable SSL
az webapp config ssl bind \
  --certificate-thumbprint <thumbprint> \
  --ssl-type SNI \
  --name app-picverse-api-prod \
  --resource-group rg-picverse-prod
```

#### 6.2 Frontend Domain
```bash
# Configure custom domain for Static Web Apps
az staticwebapp hostname set \
  --name picverse-frontend \
  --hostname www.picverse.com
```

### Step 7: Monitoring and Logging

#### 7.1 Configure Application Insights
Add Application Insights to your `Program.cs`:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

#### 7.2 Set up Alerts
```bash
# Create alert for high error rate
az monitor metrics alert create \
  --name "High Error Rate" \
  --resource-group rg-picverse-prod \
  --scopes /subscriptions/{subscription-id}/resourceGroups/rg-picverse-prod/providers/Microsoft.Web/sites/app-picverse-api-prod \
  --condition "avg exceptions/server > 10" \
  --description "Alert when error rate is high"
```

### Step 8: Security Configuration

#### 8.1 Network Security
```bash
# Configure App Service to only accept HTTPS
az webapp update \
  --resource-group rg-picverse-prod \
  --name app-picverse-api-prod \
  --https-only true

# Configure minimum TLS version
az webapp config set \
  --resource-group rg-picverse-prod \
  --name app-picverse-api-prod \
  --min-tls-version 1.2
```

#### 8.2 SQL Database Security
```bash
# Configure firewall rules
az sql server firewall-rule create \
  --resource-group rg-picverse-prod \
  --server sql-picverse-prod \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### Step 9: Performance Optimization

#### 9.1 Enable CDN
```bash
# Create CDN profile
az cdn profile create \
  --name cdn-picverse-prod \
  --resource-group rg-picverse-prod \
  --sku Standard_Microsoft

# Create CDN endpoint
az cdn endpoint create \
  --name picverse-media \
  --profile-name cdn-picverse-prod \
  --resource-group rg-picverse-prod \
  --origin stpicverseprod.blob.core.windows.net
```

#### 9.2 Configure Caching
Add caching headers in your API controllers:

```csharp
[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
public async Task<IActionResult> GetPosts()
{
    // Implementation
}
```

### Step 10: Backup and Disaster Recovery

#### 10.1 Database Backup
```bash
# Configure automated backups
az sql db ltr-policy set \
  --resource-group rg-picverse-prod \
  --server sql-picverse-prod \
  --database picverse-db \
  --weekly-retention P4W \
  --monthly-retention P12M \
  --yearly-retention P5Y
```

#### 10.2 App Service Backup
```bash
# Create storage account for backups
az storage account create \
  --name stpicversebackup \
  --resource-group rg-picverse-prod \
  --sku Standard_LRS

# Configure backup
az webapp config backup update \
  --resource-group rg-picverse-prod \
  --webapp-name app-picverse-api-prod \
  --container-url "https://stpicversebackup.blob.core.windows.net/backups" \
  --frequency 1 \
  --retain-one true \
  --retention 30
```

### Step 11: CI/CD Pipeline (Azure DevOps)

#### 11.1 Create Build Pipeline (azure-pipelines.yml)
```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

stages:
- stage: Build
  jobs:
  - job: BuildAPI
    steps:
    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
        projects: 'Backend/**/*.csproj'
    
    - task: DotNetCoreCLI@2
      displayName: 'Build API'
      inputs:
        command: 'build'
        projects: 'Backend/**/*.csproj'
        arguments: '--configuration $(buildConfiguration)'
    
    - task: DotNetCoreCLI@2
      displayName: 'Publish API'
      inputs:
        command: 'publish'
        projects: 'Backend/PicVerse.API/PicVerse.API.csproj'
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)/api'
    
    - task: PublishBuildArtifacts@1
      inputs:
        pathToPublish: '$(Build.ArtifactStagingDirectory)/api'
        artifactName: 'api'

  - job: BuildFrontend
    steps:
    - task: NodeTool@0
      inputs:
        versionSpec: '18.x'
    
    - script: |
        cd Frontend
        npm install
        npm run build --prod
      displayName: 'Build Angular app'
    
    - task: PublishBuildArtifacts@1
      inputs:
        pathToPublish: 'Frontend/dist'
        artifactName: 'frontend'

- stage: Deploy
  dependsOn: Build
  jobs:
  - deployment: DeployAPI
    environment: 'Production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: 'Azure-Connection'
              appType: 'webAppLinux'
              appName: 'app-picverse-api-prod'
              package: '$(Pipeline.Workspace)/api'
  
  - deployment: DeployFrontend
    environment: 'Production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureStaticWebApp@0
            inputs:
              app_location: '$(Pipeline.Workspace)/frontend'
              azure_static_web_apps_api_token: '$(AZURE_STATIC_WEB_APPS_API_TOKEN)'
```

### Step 12: Health Checks and Monitoring

#### 12.1 Configure Health Checks
Add to `Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddAzureSignalR(builder.Configuration.GetConnectionString("SignalRConnection"));

app.MapHealthChecks("/health");
```

#### 12.2 Application Insights Queries
Useful KQL queries for monitoring:

```kql
// Error rate
requests
| where timestamp > ago(1h)
| summarize ErrorRate = (countif(success == false) * 100.0) / count() by bin(timestamp, 5m)

// Response times
requests
| where timestamp > ago(1h)
| summarize avg(duration), percentile(duration, 95) by bin(timestamp, 5m)

// User activity
customEvents
| where name == "UserLogin"
| summarize count() by bin(timestamp, 1h)
```

### Step 13: Cost Optimization

#### 13.1 Resource Scaling
```bash
# Configure auto-scaling for App Service
az monitor autoscale create \
  --resource-group rg-picverse-prod \
  --resource /subscriptions/{subscription-id}/resourceGroups/rg-picverse-prod/providers/Microsoft.Web/serverfarms/asp-picverse-prod \
  --name autoscale-picverse \
  --min-count 1 \
  --max-count 3 \
  --count 1

# Add scale-out rule
az monitor autoscale rule create \
  --resource-group rg-picverse-prod \
  --autoscale-name autoscale-picverse \
  --condition "Percentage CPU > 70 avg 5m" \
  --scale out 1
```

### Step 14: Security Hardening

#### 14.1 Enable Advanced Threat Protection
```bash
# Enable Advanced Threat Protection for SQL Database
az sql db threat-policy update \
  --resource-group rg-picverse-prod \
  --server sql-picverse-prod \
  --database picverse-db \
  --state Enabled \
  --storage-account stpicverseprod
```

#### 14.2 Configure Web Application Firewall
```bash
# Create Application Gateway with WAF
az network application-gateway create \
  --name ag-picverse-prod \
  --resource-group rg-picverse-prod \
  --sku WAF_v2 \
  --capacity 2 \
  --vnet-name vnet-picverse \
  --subnet subnet-ag \
  --public-ip-address pip-ag-picverse
```

### Maintenance and Updates

1. **Regular Updates**: Keep all services and dependencies updated
2. **Security Patches**: Apply security patches promptly
3. **Performance Monitoring**: Regular performance reviews
4. **Cost Review**: Monthly cost analysis and optimization
5. **Backup Testing**: Regular backup restoration tests

### Troubleshooting Common Issues

1. **Connection Issues**: Check firewall rules and network security groups
2. **Authentication Failures**: Verify Key Vault permissions and secrets
3. **Performance Issues**: Review Application Insights metrics
4. **Deployment Failures**: Check build logs and deployment history

This deployment guide provides a comprehensive approach to deploying PicVerse on Azure with enterprise-grade security, scalability, and monitoring capabilities.