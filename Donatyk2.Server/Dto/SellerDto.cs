using System.ComponentModel.DataAnnotations;

namespace Donatyk2.Server.Dto
{
    public class SellerDto
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        public string AvatarImageUrl { get; set; } = string.Empty;
    }
}
