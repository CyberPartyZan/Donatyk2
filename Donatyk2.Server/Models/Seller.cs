using Donatyk2.Server.Data;

namespace Donatyk2.Server.Models
{
    public class Seller
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AvatarImageUrl { get; set; }
        public Guid UserId { get; set; }

        public Seller()
        {

        }

        public Seller(SellerEntity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            AvatarImageUrl = entity.AvatarImageUrl;
            Description = entity.Description;
            UserId = entity.UserId;
            Email = entity.Email;
            PhoneNumber = entity.PhoneNumber;
        }
    }
}
