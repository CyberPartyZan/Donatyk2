namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class SellerTests
    {
        [Fact]
        public void Constructor_WithValidData_SetsAllProperties()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var avatar = new Blob(
                Guid.NewGuid(),
                "sellers/avatars",
                "avatar-key-123",
                "avatar.png");

            var seller = new Seller(
                id,
                "Acme Corp",
                "Reliable seller",
                "seller@example.com",
                "+12345678901",
                avatar,
                userId);

            Assert.Equal(id, seller.Id);
            Assert.Equal("Acme Corp", seller.Name);
            Assert.Equal("Reliable seller", seller.Description);
            Assert.Equal("seller@example.com", seller.Email);
            Assert.Equal("+12345678901", seller.PhoneNumber);
            Assert.NotNull(seller.Avatar);
            Assert.Equal(avatar.Id, seller.Avatar!.Id);
            Assert.Equal("sellers/avatars", seller.Avatar.FilePath);
            Assert.Equal("avatar-key-123", seller.Avatar.Key);
            Assert.Equal("avatar.png", seller.Avatar.FileName);
            Assert.Equal(userId, seller.UserId);
        }

        [Fact]
        public void Constructor_WithNullAvatar_AllowsSellerWithoutAvatar()
        {
            var seller = new Seller(
                Guid.NewGuid(),
                "Acme Corp",
                "Reliable seller",
                "seller@example.com",
                "+12345678901",
                null,
                Guid.NewGuid());

            Assert.Null(seller.Avatar);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ThrowsArgumentException(string? name)
        {
            Assert.Throws<ArgumentException>(() => CreateSeller(name: name!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidDescription_ThrowsArgumentException(string? description)
        {
            Assert.Throws<ArgumentException>(() => CreateSeller(description: description!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithMissingEmail_ThrowsArgumentException(string? email)
        {
            Assert.Throws<ArgumentException>(() => CreateSeller(email: email!));
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("user@")]
        [InlineData("user@domain")]
        [InlineData("user@domain.")]
        public void Constructor_WithInvalidEmailFormat_ThrowsArgumentException(string email)
        {
            Assert.Throws<ArgumentException>(() => CreateSeller(email: email));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithMissingPhone_ThrowsArgumentException(string? phone)
        {
            Assert.Throws<ArgumentException>(() => CreateSeller(phone: phone!));
        }

        [Fact]
        public void Constructor_WithInvalidPhoneFormat_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CreateSeller(phone: "123-456"));
        }

        [Fact]
        public void Constructor_WithEmptyUserId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CreateSeller(userId: Guid.Empty));
        }

        private static Seller CreateSeller(
            string name = "Valid seller",
            string description = "Valid description",
            string email = "seller@example.com",
            string phone = "+12345678901",
            Guid? userId = null)
        {
            return new Seller(
                Guid.NewGuid(),
                name,
                description,
                email,
                phone,
                avatar: null,
                userId ?? Guid.NewGuid());
        }
    }
}