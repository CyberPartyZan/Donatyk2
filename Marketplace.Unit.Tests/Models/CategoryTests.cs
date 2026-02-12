using System;
using System.Collections.Generic;
using System.Text;
using Marketplace.Abstractions.Models;

namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class CategoryTests
    {
        [Fact]
        public void Constructor_WithValidArguments_TrimsAndInitializes()
        {
            var id = Guid.NewGuid();

            var category = new Category(id, "  Books  ", "  All kinds of books  ");

            Assert.Equal(id, category.Id);
            Assert.Equal("Books", category.Name);
            Assert.Equal("All kinds of books", category.Description);
            Assert.Empty(category.SubCategories);
            Assert.Null(category.ParentCategory);
        }

        [Fact]
        public void CreateRoot_GeneratesIdAndReturnsCategory()
        {
            var category = Category.CreateRoot("Root", "Root description");

            Assert.NotEqual(Guid.Empty, category.Id);
            Assert.Equal("Root", category.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ThrowsArgumentException(string? name)
        {
            Assert.Throws<ArgumentException>(() => new Category(Guid.NewGuid(), name!, "Valid description"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidDescription_ThrowsArgumentException(string? description)
        {
            Assert.Throws<ArgumentException>(() => new Category(Guid.NewGuid(), "Valid name", description!));
        }

        [Fact]
        public void Constructor_WithTooLongName_ThrowsArgumentException()
        {
            var longName = new string('a', 129);

            Assert.Throws<ArgumentException>(() => new Category(Guid.NewGuid(), longName, "Valid description"));
        }

        [Fact]
        public void Constructor_WithTooLongDescription_ThrowsArgumentException()
        {
            var longDescription = new string('a', 1025);

            Assert.Throws<ArgumentException>(() => new Category(Guid.NewGuid(), "Valid name", longDescription));
        }

        [Fact]
        public void AddSubCategory_WithValidChild_AddsAndSetsParent()
        {
            var parent = CreateCategory("Parent");
            var child = CreateCategory("Child");

            parent.AddSubCategory(child);

            Assert.Single(parent.SubCategories);
            Assert.Same(parent, child.ParentCategory);
        }

        [Fact]
        public void AddSubCategory_WithDuplicateName_ThrowsInvalidOperationException()
        {
            var parent = CreateCategory("Parent");
            parent.AddSubCategory("Child", "Description");

            var duplicate = CreateCategory("child");

            Assert.Throws<InvalidOperationException>(() => parent.AddSubCategory(duplicate));
        }

        [Fact]
        public void AddSubCategory_WithCircularReference_ThrowsInvalidOperationException()
        {
            var root = CreateCategory("Root");
            var child = CreateCategory("Child");
            root.AddSubCategory(child);

            Assert.Throws<InvalidOperationException>(() => child.AddSubCategory(root));
        }

        [Fact]
        public void RemoveSubCategory_WhenPresent_DetachesAndReturnsTrue()
        {
            var parent = CreateCategory("Parent");
            var child = parent.AddSubCategory("Child", "Description");

            var result = parent.RemoveSubCategory(child.Id);

            Assert.True(result);
            Assert.Empty(parent.SubCategories);
            Assert.Null(child.ParentCategory);
        }

        [Fact]
        public void RemoveSubCategory_WhenMissing_ReturnsFalse()
        {
            var parent = CreateCategory("Parent");

            var result = parent.RemoveSubCategory(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public void UpdateDetails_WithValidValues_UpdatesAndTrims()
        {
            var category = CreateCategory("Old", "Old desc");

            category.UpdateDetails("  New Name  ", "  New Description  ");

            Assert.Equal("New Name", category.Name);
            Assert.Equal("New Description", category.Description);
        }

        [Fact]
        public void FindSubCategory_FindsNestedChild()
        {
            var root = CreateCategory("Root");
            var level1 = root.AddSubCategory("Level1", "Desc");
            var level2 = level1.AddSubCategory("Level2", "Desc");

            var found = root.FindSubCategory(level2.Id);

            Assert.Same(level2, found);
        }

        private static Category CreateCategory(string name, string? description = null) =>
            new(Guid.NewGuid(), name, description ?? $"{name} description");
    }
}
