namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class ImageTests
    {
        [Fact]
        public void Constructor_WithUrlOnly_CreatesImage()
        {
            var id = Guid.NewGuid();

            var image = new Image(id, "https://example.com/image.png", null);

            Assert.Equal(id, image.Id);
            Assert.Equal("https://example.com/image.png", image.Url);
            Assert.Null(image.Data);
        }

        [Fact]
        public void Constructor_WithDataOnly_CreatesImage()
        {
            var id = Guid.NewGuid();
            var data = new byte[] { 1, 2, 3 };

            var image = new Image(id, null, data);

            Assert.Equal(id, image.Id);
            Assert.Null(image.Url);
            Assert.Equal(data, image.Data);
        }

        [Fact]
        public void Constructor_WithNoUrlAndNoData_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Image(Guid.NewGuid(), null, null));
        }
    }
}