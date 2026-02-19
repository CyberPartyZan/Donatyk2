namespace Marketplace.Repository.MSSql
{
    internal class CategoryEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public virtual CategoryEntity? ParentCategory { get; set; }
        public virtual ICollection<CategoryEntity> Subcategories { get; set; } = new List<CategoryEntity>();
    }
}
