using Marketplace.Abstractions.Models;

namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class UserTests
    {
        [Fact]
        public void Constructor_WithValidData_SetsProperties()
        {
            var id = Guid.NewGuid();
            const string email = "user@example.com";
            const bool emailConfirmed = true;
            const bool lockoutEnabled = true;
            var lockoutEnd = DateTimeOffset.UtcNow.AddDays(1);

            var user = new User(id, email, emailConfirmed, lockoutEnabled, lockoutEnd);

            Assert.Equal(id, user.Id);
            Assert.Equal(email, user.Email);
            Assert.Equal(emailConfirmed, user.EmailConfirmed);
            Assert.Equal(lockoutEnabled, user.LockoutEnabled);
            Assert.Equal(lockoutEnd, user.LockoutEnd);
        }

        [Fact]
        public void Constructor_WithEmptyGuid_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new User(Guid.Empty, "user@example.com", false, false, null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithMissingEmail_ThrowsArgumentException(string? email)
        {
            Assert.Throws<ArgumentException>(() => new User(Guid.NewGuid(), email!, false, false, null));
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("user@")]
        [InlineData("user@domain")]
        [InlineData("user@domain.")]
        public void Constructor_WithInvalidEmailFormat_ThrowsArgumentException(string email)
        {
            Assert.Throws<ArgumentException>(() => new User(Guid.NewGuid(), email, false, false, null));
        }
    }
}