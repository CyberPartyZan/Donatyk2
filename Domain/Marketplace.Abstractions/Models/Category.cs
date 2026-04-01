using System.Collections.ObjectModel;

namespace Marketplace
{
    public class Category
    {
        private readonly List<Category> _subCategories = new();
        private readonly ReadOnlyCollection<Category> _readOnlySubCategories;
        private Category? _parentCategory;

        public Category(Guid id, string name, string description)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            Name = ValidateName(name);
            Description = ValidateDescription(description);
            _readOnlySubCategories = _subCategories.AsReadOnly();
        }

        public Guid Id { get; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Category? ParentCategory => _parentCategory;
        public IReadOnlyCollection<Category> SubCategories => _readOnlySubCategories;

        public static Category CreateRoot(string name, string description) =>
            new(Guid.NewGuid(), name, description);

        public Category AddSubCategory(string name, string description) =>
            AddSubCategory(Guid.NewGuid(), name, description);

        public Category AddSubCategory(Guid id, string name, string description)
        {
            var child = new Category(id, name, description);
            AddSubCategory(child);
            return child;
        }

        public void AddSubCategory(Category category)
        {
            if (category is null)
                throw new ArgumentNullException(nameof(category));

            if (ReferenceEquals(category, this))
                throw new InvalidOperationException("Category cannot contain itself.");

            category.DetachFromParent();
            EnsureHierarchyRules(category);

            category._parentCategory = this;
            _subCategories.Add(category);
        }

        public bool RemoveSubCategory(Guid subCategoryId)
        {
            var child = _subCategories.FirstOrDefault(c => c.Id == subCategoryId);
            if (child is null)
            {
                return false;
            }

            _subCategories.Remove(child);
            child._parentCategory = null;
            return true;
        }

        public void UpdateDetails(string name, string description)
        {
            Name = ValidateName(name);
            Description = ValidateDescription(description);
        }

        public Category? FindSubCategory(Guid id)
        {
            foreach (var child in _subCategories)
            {
                if (child.Id == id)
                {
                    return child;
                }

                var nested = child.FindSubCategory(id);
                if (nested is not null)
                {
                    return nested;
                }
            }

            return null;
        }

        private void DetachFromParent()
        {
            _parentCategory?._subCategories.Remove(this);
            _parentCategory = null;
        }

        private void EnsureHierarchyRules(Category category)
        {
            if (_subCategories.Any(c => c.Id == category.Id))
                throw new InvalidOperationException($"Subcategory with id '{category.Id}' already exists.");

            if (_subCategories.Any(c => string.Equals(c.Name, category.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Subcategory with name '{category.Name}' already exists.");

            if (WouldCreateCycle(category))
                throw new InvalidOperationException("Cannot introduce circular category references.");
        }

        private bool WouldCreateCycle(Category category)
        {
            var current = this;
            while (current is not null)
            {
                if (ReferenceEquals(current, category))
                {
                    return true;
                }

                current = current._parentCategory;
            }

            return false;
        }

        private static string ValidateName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Category name cannot be null or whitespace.", nameof(value));

            var trimmed = value.Trim();
            if (trimmed.Length > 128)
                throw new ArgumentException("Category name cannot exceed 128 characters.", nameof(value));

            return trimmed;
        }

        private static string ValidateDescription(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Category description cannot be null or whitespace.", nameof(value));

            var trimmed = value.Trim();
            if (trimmed.Length > 1024)
                throw new ArgumentException("Category description cannot exceed 1024 characters.", nameof(value));

            return trimmed;
        }
    }
}
