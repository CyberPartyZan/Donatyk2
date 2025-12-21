namespace Donatyk2.Server.Models
{
    public class Seller
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AvatarImageUrl { get; set; }
        public string Description { get; set; }

        public Seller(Data.SellerEntity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            AvatarImageUrl = entity.AvatarImageUrl;
            Description = entity.Description;
        }
    }
}
