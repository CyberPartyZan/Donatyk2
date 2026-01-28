using System;
using System.Collections.Generic;

namespace Marketplace.Repository.MSSql.Entities
{
    public class CategoryEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public CategoryEntity? ParentCategory { get; set; }
        public ICollection<CategoryEntity> Subcategories { get; set; } = new List<CategoryEntity>();
    }
}
