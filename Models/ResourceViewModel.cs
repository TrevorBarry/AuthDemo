using System.ComponentModel.DataAnnotations;

namespace AzureResourceViewer.Models
{
    public class ResourceViewModel
    {
        [Display(Name = "Resource Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Resource Type")]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Resource Group")]
        public string ResourceGroupName { get; set; } = string.Empty;

        [Display(Name = "Creation Date")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Resource ID")]
        public string ResourceId { get; set; } = string.Empty;

        [Display(Name = "Tags")]
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        [Display(Name = "SKU")]
        public string? Sku { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }
    }
}
