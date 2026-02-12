using System.ComponentModel.DataAnnotations;

namespace Donatyk2.Server.Dto
{
    public class DeclineLotRequest
    {
        [Required]
        [StringLength(1024)]
        public string Reason { get; set; } = string.Empty;
    }
}