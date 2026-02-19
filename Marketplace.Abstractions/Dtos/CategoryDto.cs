using System.ComponentModel.DataAnnotations;

namespace Marketplace
{
    public class CategoryDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(1024)]
        public string Description { get; set; } = string.Empty;

        public Guid? ParentId { get; set; }

        public IList<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
    }
}