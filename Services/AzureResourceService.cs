using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureResourceViewer.Models;
using Microsoft.Identity.Web;
using Azure.Core;

namespace AzureResourceViewer.Services
{
    public interface IAzureResourceService
    {
        Task<List<ResourceGroupViewModel>> GetResourceGroupsAsync();
        Task<List<ResourceViewModel>> GetResourcesAsync(string subscriptionId, string resourceGroupName);
    }

    public class AzureResourceService : IAzureResourceService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureResourceService> _logger;

        public AzureResourceService(
            ITokenAcquisition tokenAcquisition,
            IConfiguration configuration,
            ILogger<AzureResourceService> logger)
        {
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<ResourceGroupViewModel>> GetResourceGroupsAsync()
        {
            var resourceGroups = new List<ResourceGroupViewModel>();

            try
            {
                var client = await GetArmClientAsync();
                var subscriptions = client.GetSubscriptions();

                await foreach (var subscription in subscriptions)
                {
                    _logger.LogInformation($"Processing subscription: {subscription.Data.DisplayName}");
                    
                    var resourceGroupsInSubscription = subscription.GetResourceGroups();
                    
                    foreach (var rg in resourceGroupsInSubscription)
                    {
                        try
                        {
                            // Get resource count for this resource group
                            var resourceCount = await GetResourceCountAsync(rg);
                            
                            var resourceGroupViewModel = new ResourceGroupViewModel
                            {
                                Name = rg.Data.Name,
                                Location = rg.Data.Location.ToString(),
                                SubscriptionId = subscription.Data.SubscriptionId,
                                ResourceCount = resourceCount,
                                CreatedDate = null, // CreatedOn property may not be available in all SDK versions
                                Tags = rg.Data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>()
                            };

                            resourceGroups.Add(resourceGroupViewModel);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing resource group {rg.Data.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource groups");
                throw;
            }

            return resourceGroups.OrderBy(rg => rg.Name).ToList();
        }

        public async Task<List<ResourceViewModel>> GetResourcesAsync(string subscriptionId, string resourceGroupName)
        {
            var resources = new List<ResourceViewModel>();

            try
            {
                var client = await GetArmClientAsync();
                var subscription = client.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId));
                var resourceGroup = await subscription.GetResourceGroupAsync(resourceGroupName);

                if (resourceGroup?.Value != null)
                {
                    var genericResources = resourceGroup.Value.GetGenericResources();

                    foreach (var resource in genericResources)
                    {
                        try
                        {
                            var resourceViewModel = new ResourceViewModel
                            {
                                Name = resource.Data.Name,
                                Type = resource.Data.ResourceType.ToString(),
                                Location = resource.Data.Location.ToString() ?? "N/A",
                                ResourceGroupName = resourceGroupName,
                                CreatedDate = null, // CreatedOn property may not be available
                                ResourceId = resource.Data.Id.ToString(),
                                Tags = resource.Data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>(),
                                Sku = resource.Data.Sku?.Name ?? "N/A"
                            };

                            resources.Add(resourceViewModel);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing resource {resource.Data.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting resources for resource group {resourceGroupName}");
                throw;
            }

            return resources.OrderBy(r => r.Name).ToList();
        }

        private async Task<ArmClient> GetArmClientAsync()
        {
            try
            {
                // Get access token from Microsoft Identity Web
                var scopes = new[] { "https://management.azure.com/user_impersonation" };
                var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                
                // Create credential using the token
                var credential = new OnBehalfOfCredential(
                    new AccessToken(accessToken, DateTimeOffset.UtcNow.AddHours(1)));
                
                return new ArmClient(credential);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ARM client");
                // Fallback to DefaultAzureCredential for development
                return new ArmClient(new DefaultAzureCredential());
            }
        }

        private Task<int> GetResourceCountAsync(ResourceGroupResource resourceGroup)
        {
            try
            {
                var count = 0;
                var resources = resourceGroup.GetGenericResources();
                
                foreach (var resource in resources)
                {
                    count++;
                }
                
                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Could not get resource count for {resourceGroup.Data.Name}");
                return Task.FromResult(0);
            }
        }
    }

    // Custom credential for token-based authentication
    public class OnBehalfOfCredential : TokenCredential
    {
        private readonly AccessToken _accessToken;

        public OnBehalfOfCredential(AccessToken accessToken)
        {
            _accessToken = accessToken;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return _accessToken;
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new ValueTask<AccessToken>(_accessToken);
        }
    }
}
