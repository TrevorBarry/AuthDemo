# Azure Resource Viewer

A .NET Core web application that uses Microsoft Entra ID (Azure AD) authentication to display Azure resource groups and their resources.

## Features

- **Secure Authentication**: Uses Microsoft Entra ID for user authentication
- **Resource Group Listing**: Displays all accessible Azure resource groups with resource counts
- **Resource Details**: Shows detailed information about resources within each resource group
- **Modern UI**: Bootstrap-based responsive interface with Font Awesome icons
- **Security First**: Follows Azure best practices for authentication and authorization

## Prerequisites

1. **Azure Subscription**: You need an active Azure subscription
2. **Azure AD App Registration**: You need to register this application in Azure AD
3. **.NET 9.0**: The application is built on .NET 9.0

## Setup Instructions

### 1. Azure AD App Registration

1. Go to the [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Configure the registration:
   - **Name**: `Azure Resource Viewer`
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: 
     - Type: Web
     - URI: `https://localhost:7000/signin-oidc` (adjust port if different)
5. Click **Register**

### 2. Configure App Registration

After registration, note down:
- **Application (client) ID**
- **Directory (tenant) ID**

#### Configure Authentication:
1. Go to **Authentication** in your app registration
2. Add redirect URIs:
   - `https://localhost:7000/signin-oidc`
   - `https://localhost:7000/signout-callback-oidc`
3. Enable **ID tokens** under Implicit grant and hybrid flows

#### Configure API Permissions:
1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Azure Service Management**
4. Select **Delegated permissions**
5. Check **user_impersonation**
6. Click **Add permissions**
7. Click **Grant admin consent** for your organization

### 3. Configure Application Settings

Update the `appsettings.json` file with your Azure AD information:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "your-tenant-domain.onmicrosoft.com",
    "TenantId": "your-tenant-id-from-step-2",
    "ClientId": "your-client-id-from-step-2",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  },
  "Azure": {
    "SubscriptionId": "your-subscription-id"
  }
}
```

### 4. RBAC Permissions

Ensure your Azure AD user has appropriate permissions to view Azure resources:

1. Go to your Azure subscription in the Azure Portal
2. Navigate to **Access control (IAM)**
3. Add role assignment:
   - **Role**: Reader (minimum) or Contributor
   - **Assign access to**: User, group, or service principal
   - **Select**: Your user account

## Running the Application

1. Clone or download the application
2. Configure the settings as described above
3. Run the application:
   ```bash
   dotnet run
   ```
4. Navigate to `https://localhost:7000` (or the configured port)
5. Sign in with your Azure AD credentials

## Application Structure

- **Controllers**: 
  - `ResourceGroupsController`: Handles resource group and resource operations
- **Services**: 
  - `IAzureResourceService`: Interface for Azure operations
  - `AzureResourceService`: Implementation using Azure.ResourceManager SDK
- **Models**: 
  - `ResourceGroupViewModel`: Resource group display model
  - `ResourceViewModel`: Resource display model
- **Views**: 
  - Resource group listing and resource detail pages

## Security Features

- **Authentication**: Microsoft Entra ID integration
- **Authorization**: Requires authenticated users
- **Token Management**: Secure token handling with Microsoft.Identity.Web
- **RBAC**: Uses Azure role-based access control
- **Secure Communication**: HTTPS enforcement

## Technology Stack

- **.NET 9.0**: Latest .NET framework
- **ASP.NET Core MVC**: Web framework
- **Microsoft.Identity.Web**: Authentication library
- **Azure.ResourceManager**: Azure SDK for resource management
- **Bootstrap 5**: UI framework
- **Font Awesome**: Icons

## Troubleshooting

### Common Issues

1. **Authentication fails**: 
   - Verify Azure AD app registration settings
   - Check redirect URIs match exactly
   - Ensure API permissions are granted

2. **No resource groups shown**:
   - Verify user has Reader access to Azure subscription
   - Check subscription ID in configuration
   - Review application logs for errors

3. **Permission errors**:
   - Ensure user has appropriate RBAC permissions
   - Verify API permissions in Azure AD app registration

### Logging

The application includes comprehensive logging. Check the console output for detailed error messages and diagnostic information.

## Production Deployment

For production deployment:

1. **Use Azure Key Vault** for sensitive configuration
2. **Configure proper SSL certificates**
3. **Set up Application Insights** for monitoring
4. **Configure proper logging** levels
5. **Use managed identity** when deployed to Azure

## License

This project is for educational purposes. Please review and comply with Microsoft's licensing terms for Azure SDK usage.
