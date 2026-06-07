namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class CharacteristicTests
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsProperties()
        {
            var characteristic = new Characteristic("Color", "Red");

            Assert.Equal("Color", characteristic.Key);
            Assert.Equal("Red", characteristic.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidKey_ThrowsArgumentException(string? key)
        {
            Assert.Throws<ArgumentException>(() => new Characteristic(key!, "Value"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidValue_ThrowsArgumentException(string? value)
        {
            Assert.Throws<ArgumentException>(() => new Characteristic("Key", value!));
        }
    }
}