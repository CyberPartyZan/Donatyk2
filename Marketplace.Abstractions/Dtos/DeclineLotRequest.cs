using System.ComponentModel.DataAnnotations;

namespace Marketplace
{
    public class DeclineLotRequest
    {
        [Required]
        [StringLength(1024)]
        public string Reason { get; set; } = string.Empty;
    }
}