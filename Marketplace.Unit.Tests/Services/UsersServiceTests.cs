using AutoFixture;
using AutoFixture.AutoMoq;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Repositories.Interfaces;
using Donatyk2.Server.Services;
using Donatyk2.Server.Services.Interfaces;
using Marketplace.Abstractions.Models;
using Moq;

namespace Marketplace.Unit.Tests.Services
{
    public sealed class UsersServiceTests
    {
        [Fact]
        public async Task GetAll_ReturnsMappedDtos()
        {
            var fixture = CreateFixture();
            var users = new[]
            {
                CreateUser(email: "first@example.com"),
                CreateUser(email: "second@example.com", emailConfirmed: true, lockoutEnabled: true)
            };

            var repo = fixture.Freeze<Mock<IUsersRepository>>();
            repo.Setup(r => r.GetAll("query", 2, 50)).ReturnsAsync(users);

            var service = fixture.Create<UsersService>();

            var result = (await service.GetAll("query", 2, 50)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(users[0].Id, result[0].Id);
            Assert.Equal(users[1].EmailConfirmed, result[1].EmailConfirmed);
            repo.Verify(r => r.GetAll("query", 2, 50), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenFound_ReturnsDto()
        {
            var fixture = CreateFixture();
            var user = CreateUser();
            var repo = fixture.Freeze<Mock<IUsersRepository>>();
            repo.Setup(r => r.GetById(user.Id)).ReturnsAsync(user);

            var service = fixture.Create<UsersService>();

            var result = await service.GetById(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
            Assert.Equal(user.LockoutEnd, result.LockoutEnd);
        }

        [Fact]
        public async Task GetById_WhenMissing_ReturnsNull()
        {
            var fixture = CreateFixture();
            var repo = fixture.Freeze<Mock<IUsersRepository>>();
            repo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((User?)null);

            var service = fixture.Create<UsersService>();

            var result = await service.GetById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmail_WhenFound_ReturnsDto()
        {
            var fixture = CreateFixture();
            var user = CreateUser(email: "user@example.com");
            var repo = fixture.Freeze<Mock<IUsersRepository>>();
            repo.Setup(r => r.GetByEmail(user.Email)).ReturnsAsync(user);

            var service = fixture.Create<UsersService>();

            var result = await service.GetByEmail(user.Email);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);
        }

        [Fact]
        public async Task GetByEmail_WhenMissing_ReturnsNull()
        {
            var fixture = CreateFixture();
            var repo = fixture.Freeze<Mock<IUsersRepository>>();
            repo.Setup(r => r.GetByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);

            var service = fixture.Create<UsersService>();

            var result = await service.GetByEmail("missing@example.com");

            Assert.Null(result);
        }

        [Fact]
        public async Task Update_WhenUserMissing_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            var repo = fixture.Freeze<Mock<IUsersRepository>>();
            repo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((User?)null);

            var service = fixture.Create<UsersService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Update(CreateUserDto()));
        }

        [Fact]
        public async Task Update_WithExistingUser_AppliesChanges()
        {
            var fixture = CreateFixture();
            var existing = CreateUser(email: "old@example.com", emailConfirmed: false, lockoutEnabled: false, lockoutEnd: DateTimeOffset.UtcNow.AddDays(1));
            var repo = fixture.Freeze<Mock<IUsersRepository>>();
            repo.Setup(r => r.GetById(existing.Id)).ReturnsAsync(existing);

            var dto = new UserDto
            {
                Id = existing.Id,
                Email = "new@example.com",
                EmailConfirmed = true,
                LockoutEnabled = true,
                LockoutEnd = DateTimeOffset.UtcNow.AddDays(5)
            };

            var service = fixture.Create<UsersService>();

            await service.Update(dto);

            repo.Verify(r => r.Update(It.Is<User>(u =>
                u == existing &&
                u.Email == dto.Email &&
                u.EmailConfirmed == dto.EmailConfirmed &&
                u.LockoutEnabled == dto.LockoutEnabled &&
                u.LockoutEnd == dto.LockoutEnd)), Times.Once);
        }

        [Fact]
        public async Task Update_WithNullLockoutEnd_PreservesExistingValue()
        {
            var fixture = CreateFixture();
            var originalLockout = DateTimeOffset.UtcNow.AddDays(3);
            var existing = CreateUser(lockoutEnd: originalLockout);
            var repo = fixture.Freeze<Mock<IUsersRepository>>();
            repo.Setup(r => r.GetById(existing.Id)).ReturnsAsync(existing);

            var dto = new UserDto
            {
                Id = existing.Id,
                Email = "updated@example.com",
                EmailConfirmed = existing.EmailConfirmed,
                LockoutEnabled = existing.LockoutEnabled,
                LockoutEnd = null
            };

            var service = fixture.Create<UsersService>();

            await service.Update(dto);

            repo.Verify(r => r.Update(It.Is<User>(u =>
                u.LockoutEnd == originalLockout &&
                u.Email == dto.Email)), Times.Once);
        }

        [Fact]
        public async Task Delete_RemovesUserAndSeller()
        {
            var fixture = CreateFixture();
            var id = fixture.Create<Guid>();

            var repo = fixture.Freeze<Mock<IUsersRepository>>();
            var sellersService = fixture.Freeze<Mock<ISellersService>>();

            var service = fixture.Create<UsersService>();

            await service.Delete(id);

            repo.Verify(r => r.Delete(id), Times.Once);
            sellersService.Verify(s => s.DeleteByUserId(id), Times.Once);
        }

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static User CreateUser(string? email = null, bool emailConfirmed = false, bool lockoutEnabled = false, DateTimeOffset? lockoutEnd = null)
        {
            return new User(
                Guid.NewGuid(),
                email ?? $"user-{Guid.NewGuid():N}@example.com",
                emailConfirmed,
                lockoutEnabled,
                lockoutEnd);
        }

        private static UserDto CreateUserDto(Guid? id = null) =>
            new()
            {
                Id = id ?? Guid.NewGuid(),
                Email = "dto@example.com",
                EmailConfirmed = true,
                LockoutEnabled = true,
                LockoutEnd = DateTimeOffset.UtcNow
            };
    }
}