using System.ComponentModel.DataAnnotations;

namespace AzureResourceViewer.Models
{
    public class ResourceGroupViewModel
    {
        [Display(Name = "Resource Group Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Subscription ID")]
        public string SubscriptionId { get; set; } = string.Empty;

        [Display(Name = "Resource Count")]
        public int ResourceCount { get; set; }

        [Display(Name = "Creation Date")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Tags")]
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }
}
