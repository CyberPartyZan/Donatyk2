namespace Marketplace.Repository.MSSql
{
    internal class SellerEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? AvatarImageUrl { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
