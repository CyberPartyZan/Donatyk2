using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using Marketplace.BlobStorage;
using Marketplace.Repository;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace Marketplace.Unit.Tests.Services
{
    public sealed class SellersServiceTests
    {
        [Fact]
        public async Task GetAll_ReturnsMappedDtos()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var sellers = new[]
            {
                CreateSeller(name: "First", description: "First description", avatar: CreateBlob("first.png")),
                CreateSeller(name: "Second", description: "Second description", avatar: CreateBlob("second.png"))
            };

            var repo = fixture.Freeze<Mock<ISellersRepository>>();
            repo.Setup(r => r.GetAll("abc", 2, 25)).ReturnsAsync(sellers);

            var service = fixture.Create<SellersService>();

            var result = (await service.GetAll("abc", 2, 25)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(sellers[0].Name, result[0].Name);
            Assert.Equal(sellers[1].Email, result[1].Email);
            Assert.NotNull(result[0].Avatar);
            Assert.Equal("first.png", result[0].Avatar!.FileName);

            repo.Verify(r => r.GetAll("abc", 2, 25), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenSellerExists_ReturnsDto()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var seller = CreateSeller(avatar: CreateBlob("avatar.png"));
            var repo = fixture.Freeze<Mock<ISellersRepository>>();
            repo.Setup(r => r.GetById(seller.Id)).ReturnsAsync(seller);

            var service = fixture.Create<SellersService>();

            var result = await service.GetById(seller.Id);

            Assert.NotNull(result);
            Assert.Equal(seller.Id, result!.Id);
            Assert.Equal(seller.PhoneNumber, result.PhoneNumber);
            Assert.NotNull(result.Avatar);
            Assert.Equal("avatar.png", result.Avatar!.FileName);
        }

        [Fact]
        public async Task GetById_WhenMissing_ReturnsNull()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var repo = fixture.Freeze<Mock<ISellersRepository>>();
            repo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Seller?)null);

            var service = fixture.Create<SellersService>();

            var result = await service.GetById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Create_WithValidDto_UsesCurrentUserId_AndMapsAvatar()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var dto = CreateSellerDto(CreateBlobDto("create-avatar.png"));
            Seller? captured = null;

            var repo = fixture.Freeze<Mock<ISellersRepository>>();
            repo.Setup(r => r.Create(It.IsAny<Seller>()))
                .Callback<Seller>(seller => captured = seller)
                .ReturnsAsync((Seller seller) => seller.Id);

            var service = fixture.Create<SellersService>();

            var result = await service.Create(dto);

            Assert.NotNull(captured);
            Assert.Equal(captured!.Id, result);
            Assert.Equal(dto.Name, captured.Name);
            Assert.Equal(userId, captured.UserId);
            Assert.NotNull(captured.Avatar);
            Assert.Equal("create-avatar.png", captured.Avatar!.FileName);

            repo.Verify(r => r.Create(It.IsAny<Seller>()), Times.Once);
        }

        [Fact]
        public async Task Create_WithoutUserIdClaim_ThrowsArgumentNullException()
        {
            var fixture = CreateFixture();
            fixture.Inject(new ClaimsPrincipal(new ClaimsIdentity()));

            var service = fixture.Create<SellersService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.Create(CreateSellerDto()));
        }

        [Fact]
        public async Task Update_WhenSellerMissing_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var repo = fixture.Freeze<Mock<ISellersRepository>>();
            repo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Seller?)null);

            var service = fixture.Create<SellersService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Update(Guid.NewGuid(), CreateSellerDto()));
        }

        [Fact]
        public async Task Update_WithExistingSeller_PreservesUserId_AndUsesDtoAvatarWhenProvided()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var existing = CreateSeller(userId: Guid.NewGuid(), avatar: CreateBlob("old-avatar.png"));
            Seller? captured = null;

            var repo = fixture.Freeze<Mock<ISellersRepository>>();
            repo.Setup(r => r.GetById(existing.Id)).ReturnsAsync(existing);
            repo.Setup(r => r.Update(It.IsAny<Seller>()))
                .Callback<Seller>(seller => captured = seller)
                .Returns(Task.CompletedTask);

            var dto = CreateSellerDto(CreateBlobDto("new-avatar.png"));
            var service = fixture.Create<SellersService>();

            await service.Update(existing.Id, dto);

            Assert.NotNull(captured);
            Assert.Equal(existing.Id, captured!.Id);
            Assert.Equal(existing.UserId, captured.UserId);
            Assert.NotNull(captured.Avatar);
            Assert.Equal("new-avatar.png", captured.Avatar!.FileName);
        }

        [Fact]
        public async Task UploadAvatar_ReturnsBlobDto()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var blobStorage = fixture.Freeze<Mock<IBlobStorageService>>();
            blobStorage.Setup(x => x.UploadAsync(It.IsAny<Stream>(), "sellers/avatars"))
                .ReturnsAsync("uploaded-key");

            var service = fixture.Create<SellersService>();
            using var stream = new MemoryStream([1, 2, 3]);

            var result = await service.UploadAvatar(stream, "avatar.png");

            Assert.Equal("sellers/avatars", result.FilePath);
            Assert.Equal("uploaded-key", result.Key);
            Assert.Equal("avatar.png", result.FileName);
        }

        [Fact]
        public async Task Delete_InvokesRepository()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var repo = fixture.Freeze<Mock<ISellersRepository>>();
            var service = fixture.Create<SellersService>();
            var id = fixture.Create<Guid>();

            await service.Delete(id);

            repo.Verify(r => r.Delete(id), Times.Once);
        }

        [Fact]
        public async Task DeleteByUserId_WhenSellerFound_DeletesSeller()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var seller = CreateSeller();
            var repo = fixture.Freeze<Mock<ISellersRepository>>();
            repo.Setup(r => r.GetByUserId(seller.UserId)).ReturnsAsync(seller);

            var service = fixture.Create<SellersService>();

            await service.DeleteByUserId(seller.UserId);

            repo.Verify(r => r.Delete(seller.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteByUserId_WhenSellerMissing_DoesNothing()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var repo = fixture.Freeze<Mock<ISellersRepository>>();
            repo.Setup(r => r.GetByUserId(It.IsAny<Guid>())).ReturnsAsync((Seller?)null);

            var service = fixture.Create<SellersService>();

            await service.DeleteByUserId(Guid.NewGuid());

            repo.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
        }

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static ClaimsPrincipal CreatePrincipal(Guid userId)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));
            return new ClaimsPrincipal(identity);
        }

        private static Seller CreateSeller(string? name = null, string? description = null, Guid? userId = null, Blob? avatar = null)
        {
            return new Seller(
                Guid.NewGuid(),
                name ?? "Seller " + Guid.NewGuid().ToString("N"),
                description ?? "Description " + Guid.NewGuid().ToString("N"),
                "seller@example.com",
                "+12345678901",
                avatar,
                userId ?? Guid.NewGuid());
        }

        private static Blob CreateBlob(string fileName) =>
            new(Guid.NewGuid(), "sellers/avatars", Guid.NewGuid().ToString("N"), fileName);

        private static BlobDto CreateBlobDto(string fileName) =>
            new()
            {
                Id = Guid.NewGuid(),
                FilePath = "sellers/avatars",
                Key = Guid.NewGuid().ToString("N"),
                FileName = fileName
            };

        private static SellerDto CreateSellerDto(BlobDto? avatar = null) =>
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Seller DTO",
                Description = "DTO Description",
                Email = "seller@example.com",
                PhoneNumber = "+12345678901",
                Avatar = avatar
            };
    }
}