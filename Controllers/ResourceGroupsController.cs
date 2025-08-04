using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AzureResourceViewer.Services;
using AzureResourceViewer.Models;

namespace AzureResourceViewer.Controllers
{
    [Authorize]
    public class ResourceGroupsController : Controller
    {
        private readonly IAzureResourceService _azureResourceService;
        private readonly ILogger<ResourceGroupsController> _logger;

        public ResourceGroupsController(
            IAzureResourceService azureResourceService,
            ILogger<ResourceGroupsController> logger)
        {
            _azureResourceService = azureResourceService;
            _logger = logger;
        }

        // GET: ResourceGroups
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching resource groups for user {User}", User.Identity?.Name);
                var resourceGroups = await _azureResourceService.GetResourceGroupsAsync();
                return View(resourceGroups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching resource groups");
                TempData["ErrorMessage"] = "Unable to fetch resource groups. Please check your Azure permissions and try again.";
                return View(new List<ResourceGroupViewModel>());
            }
        }

        // GET: ResourceGroups/Resources/subscriptionId/resourceGroupName
        [Route("ResourceGroups/Resources/{subscriptionId}/{resourceGroupName}")]
        public async Task<IActionResult> Resources(string subscriptionId, string resourceGroupName)
        {
            try
            {
                _logger.LogInformation("Fetching resources for resource group {ResourceGroup} in subscription {Subscription}", 
                    resourceGroupName, subscriptionId);
                
                var resources = await _azureResourceService.GetResourcesAsync(subscriptionId, resourceGroupName);
                
                ViewBag.ResourceGroupName = resourceGroupName;
                ViewBag.SubscriptionId = subscriptionId;
                
                return View(resources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching resources for resource group {ResourceGroup}", resourceGroupName);
                TempData["ErrorMessage"] = $"Unable to fetch resources for resource group '{resourceGroupName}'. Please check your Azure permissions and try again.";
                return View(new List<ResourceViewModel>());
            }
        }
    }
}
