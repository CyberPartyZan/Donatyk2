using AutoFixture;
using AutoFixture.AutoMoq;
using Marketplace.Repository;
using Moq;

namespace Marketplace.Unit.Tests.Services
{
    public sealed class CategoryServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsMappedDtos()
        {
            var fixture = CreateFixture();
            var parent = CreateCategory("Parent", "Parent description");
            var child = parent.AddSubCategory(Guid.NewGuid(), "Child", "Child description");

            var categoriesRepository = fixture.Freeze<Mock<ICategoriesRepository>>();
            categoriesRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { parent });

            var service = fixture.Create<CategoryService>();

            var result = await service.GetAllAsync();

            var dto = Assert.Single(result);
            Assert.Equal(parent.Id, dto.Id);
            Assert.Null(dto.ParentId);

            var childDto = Assert.Single(dto.SubCategories);
            Assert.Equal(child.Id, childDto.Id);
            Assert.Equal(parent.Id, childDto.ParentId);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCategoryFound_ReturnsDto()
        {
            var fixture = CreateFixture();
            var category = CreateCategory("Gadgets", "All gadgets");

            var repo = fixture.Freeze<Mock<ICategoriesRepository>>();
            repo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(category);

            var service = fixture.Create<CategoryService>();

            var result = await service.GetByIdAsync(category.Id);

            Assert.NotNull(result);
            Assert.Equal(category.Id, result!.Id);
            repo.Verify(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCategoryMissing_ReturnsNull()
        {
            var fixture = CreateFixture();
            var repo = fixture.Freeze<Mock<ICategoriesRepository>>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            var service = fixture.Create<CategoryService>();

            var result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_WithNullDto_ThrowsArgumentNullException()
        {
            var fixture = CreateFixture();
            var service = fixture.Create<CategoryService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_WithSelfParent_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            var categoryId = fixture.Create<Guid>();
            var dto = CreateCategoryDto(categoryId);
            dto.Id = categoryId;
            dto.ParentId = categoryId;

            var service = fixture.Create<CategoryService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_WithValidDto_PassesCategoryToRepository()
        {
            var fixture = CreateFixture();
            var dto = CreateCategoryDto(fixture.Create<Guid>());
            var repo = fixture.Freeze<Mock<ICategoriesRepository>>();

            Category? capturedCategory = null;
            Guid? capturedParent = null;
            repo.Setup(r => r.CreateAsync(It.IsAny<Category>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category category, Guid? parent, CancellationToken _) =>
                {
                    capturedCategory = category;
                    capturedParent = parent;
                    return category;
                });

            var service = fixture.Create<CategoryService>();

            var result = await service.CreateAsync(dto);

            Assert.NotNull(capturedCategory);
            Assert.Equal(dto.Id, capturedCategory!.Id);
            Assert.Equal(dto.Name, capturedCategory.Name);
            Assert.Equal(dto.Description, capturedCategory.Description);
            Assert.Equal(dto.ParentId, capturedParent);
            Assert.Equal(dto.Id, result.Id);
        }

        [Fact]
        public async Task UpdateAsync_WithNullDto_ThrowsArgumentNullException()
        {
            var fixture = CreateFixture();
            var service = fixture.Create<CategoryService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAsync(Guid.NewGuid(), null!));
        }

        [Fact]
        public async Task UpdateAsync_WithSelfParent_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            var id = fixture.Create<Guid>();
            var dto = CreateCategoryDto(id);
            dto.ParentId = id;

            var service = fixture.Create<CategoryService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(id, dto));
        }

        [Fact]
        public async Task UpdateAsync_WithValidDto_PassesCategoryToRepository()
        {
            var fixture = CreateFixture();
            var id = fixture.Create<Guid>();
            var dto = CreateCategoryDto(fixture.Create<Guid>());
            var repo = fixture.Freeze<Mock<ICategoriesRepository>>();

            Category? capturedCategory = null;
            Guid? capturedParent = null;
            repo.Setup(r => r.UpdateAsync(It.IsAny<Category>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .Callback<Category, Guid?, CancellationToken>((category, parent, _) =>
                {
                    capturedCategory = category;
                    capturedParent = parent;
                })
                .Returns(Task.CompletedTask);

            var service = fixture.Create<CategoryService>();

            await service.UpdateAsync(id, dto);

            Assert.NotNull(capturedCategory);
            Assert.Equal(id, capturedCategory!.Id);
            Assert.Equal(dto.Name, capturedCategory.Name);
            Assert.Equal(dto.Description, capturedCategory.Description);
            Assert.Equal(dto.ParentId, capturedParent);
            repo.Verify(r => r.UpdateAsync(It.IsAny<Category>(), dto.ParentId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_InvokesRepository()
        {
            var fixture = CreateFixture();
            var repo = fixture.Freeze<Mock<ICategoriesRepository>>();
            var service = fixture.Create<CategoryService>();
            var id = fixture.Create<Guid>();

            await service.DeleteAsync(id);

            repo.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static Category CreateCategory(string name, string description) =>
            new(Guid.NewGuid(), name, description);

        private static CategoryDto CreateCategoryDto(Guid? parentId = null) =>
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Category " + Guid.NewGuid().ToString("N"),
                Description = "Description " + Guid.NewGuid().ToString("N"),
                ParentId = parentId
            };
    }
}