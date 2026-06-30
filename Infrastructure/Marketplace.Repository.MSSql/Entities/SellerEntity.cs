namespace Marketplace.Repository.MSSql
{
    internal class SellerEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Guid? AvatarId { get; set; }
        public BlobEntity? Avatar { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<OrderEntity> Orders { get; set; } = new List<OrderEntity>();
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
