# Azure AD App Registration Setup Script
# This script helps you create an Azure AD app registration for the Azure Resource Viewer

param(
    [Parameter(Mandatory=$false)]
    [string]$AppName = "Azure Resource Viewer",
    
    [Parameter(Mandatory=$false)]
    [string]$RedirectUri = "https://localhost:7000"
)

Write-Host "Azure Resource Viewer - Azure AD Setup Script" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host ""

# Check if Azure CLI is installed
try {
    az version | Out-Null
    Write-Host "✓ Azure CLI is installed" -ForegroundColor Green
} catch {
    Write-Host "✗ Azure CLI is not installed. Please install it from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Red
    exit 1
}

# Check if user is logged in
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Please log in to Azure CLI first:" -ForegroundColor Yellow
    Write-Host "az login" -ForegroundColor White
    exit 1
}

Write-Host "✓ Logged in as: $($account.user.name)" -ForegroundColor Green
Write-Host "✓ Subscription: $($account.name) ($($account.id))" -ForegroundColor Green
Write-Host ""

# Create the app registration
Write-Host "Creating Azure AD app registration..." -ForegroundColor Yellow

$redirectUris = @(
    "$RedirectUri/signin-oidc",
    "$RedirectUri/signout-callback-oidc"
)

try {
    # Create the app registration
    $app = az ad app create `
        --display-name $AppName `
        --web-redirect-uris $redirectUris `
        --enable-id-token-issuance true `
        --query "{appId:appId,id:id}" `
        -o json | ConvertFrom-Json

    Write-Host "✓ App registration created successfully!" -ForegroundColor Green
    Write-Host "  App ID: $($app.appId)" -ForegroundColor White
    
    # Add required API permissions
    Write-Host "Adding API permissions..." -ForegroundColor Yellow
    
    # Azure Service Management API permission
    az ad app permission add `
        --id $app.appId `
        --api 797f4846-ba00-40b8-bd07-b4c1bd500f64 `
        --api-permissions 41094075-9dad-400e-a0bd-54e686782033=Scope | Out-Null
    
    Write-Host "✓ API permissions added" -ForegroundColor Green
    
    # Grant admin consent
    Write-Host "Granting admin consent..." -ForegroundColor Yellow
    az ad app permission admin-consent --id $app.appId | Out-Null
    Write-Host "✓ Admin consent granted" -ForegroundColor Green
    
    # Get tenant information
    $tenant = az account show --query "tenantId" -o tsv
    $tenantDetails = az rest --method GET --url "https://graph.microsoft.com/v1.0/organization" --query "value[0].{domain:verifiedDomains[?isDefault].name | [0], displayName:displayName}" -o json | ConvertFrom-Json
    
    Write-Host ""
    Write-Host "======================================================" -ForegroundColor Cyan
    Write-Host "SUCCESS! Your Azure AD app registration is ready!" -ForegroundColor Green
    Write-Host "======================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Configuration values for your appsettings.json:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  ""AzureAd"": {" -ForegroundColor White
    Write-Host "    ""Instance"": ""https://login.microsoftonline.com/""," -ForegroundColor White
    Write-Host "    ""Domain"": ""$($tenantDetails.domain)""," -ForegroundColor White
    Write-Host "    ""TenantId"": ""$tenant""," -ForegroundColor White
    Write-Host "    ""ClientId"": ""$($app.appId)""," -ForegroundColor White
    Write-Host "    ""CallbackPath"": ""/signin-oidc""," -ForegroundColor White
    Write-Host "    ""SignedOutCallbackPath"": ""/signout-callback-oidc""" -ForegroundColor White
    Write-Host "  }," -ForegroundColor White
    Write-Host "  ""Azure"": {" -ForegroundColor White
    Write-Host "    ""SubscriptionId"": ""$($account.id)""" -ForegroundColor White
    Write-Host "  }" -ForegroundColor White
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Update your appsettings.json or appsettings.Development.json with the above values" -ForegroundColor White
    Write-Host "2. Ensure your user account has 'Reader' role on the Azure subscription" -ForegroundColor White
    Write-Host "3. Run the application: dotnet run" -ForegroundColor White
    Write-Host "4. Navigate to: $RedirectUri" -ForegroundColor White
    Write-Host ""
    
} catch {
    Write-Host "✗ Error creating app registration: $_" -ForegroundColor Red
    exit 1
}

Write-Host "Setup completed successfully!" -ForegroundColor Green
