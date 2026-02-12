using System;
using System.Collections.Generic;
using System.Text;
using Donatyk2.Server.Enums;
using Donatyk2.Server.Models;
using Donatyk2.Server.ValueObjects;
using Xunit;

namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class LotTests
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsPropertiesAndProfit()
        {
            var id = Guid.NewGuid();
            var seller = CreateSeller();
            var price = new Money(150m, Currency.USD);
            var compensation = new Money(110m, Currency.USD);
            var stockCount = 5;
            const double discount = 12.5;
            const LotType type = LotType.Auction;
            const LotStage stage = LotStage.PendingApproval;
            const bool isActive = true;
            const bool isCompensationPaid = false;

            var lot = new Lot(id, "Limited edition cap", "Signed by the author", price, compensation, stockCount, discount, type, stage, seller, isActive, isCompensationPaid);

            Assert.Equal(id, lot.Id);
            Assert.Equal("Limited edition cap", lot.Name);
            Assert.Equal("Signed by the author", lot.Description);
            Assert.Equal(price, lot.Price);
            Assert.Equal(compensation, lot.Compensation);
            Assert.Equal(stockCount, lot.StockCount);
            Assert.Equal(discount, lot.Discount);
            Assert.Equal(type, lot.Type);
            Assert.Equal(stage, lot.Stage);
            Assert.Equal(seller, lot.Seller);
            Assert.True(lot.IsActive);
            Assert.False(lot.IsCompensationPaid);
            Assert.Equal(price - compensation, lot.Profit);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ThrowsArgumentException(string? name)
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentException>(() =>
                new Lot(Guid.NewGuid(), name!, "Valid description", CreateMoney(100m), CreateMoney(80m), 1, 0, LotType.Simple, LotStage.Created, seller, true, false));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidDescription_ThrowsArgumentException(string? description)
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentException>(() =>
                new Lot(Guid.NewGuid(), "Valid name", description!, CreateMoney(100m), CreateMoney(80m), 1, 0, LotType.Simple, LotStage.Created, seller, true, false));
        }

        [Fact]
        public void Constructor_WithNegativePrice_ThrowsArgumentOutOfRangeException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", new Money(-1m, Currency.USD), CreateMoney(0m), 1, 0, LotType.Simple, LotStage.Created, seller, true, false));
        }

        [Fact]
        public void Constructor_WithNegativeCompensation_ThrowsArgumentOutOfRangeException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(10m), new Money(-1m, Currency.USD), 1, 0, LotType.Simple, LotStage.Created, seller, true, false));
        }

        [Fact]
        public void Constructor_WithNegativeStockCount_ThrowsArgumentOutOfRangeException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(10m), CreateMoney(5m), -1, 0, LotType.Simple, LotStage.Created, seller, true, false));
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(100.01)]
        public void Constructor_WithDiscountOutsideRange_ThrowsArgumentOutOfRangeException(double discount)
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(10m), CreateMoney(5m), 1, discount, LotType.Simple, LotStage.Created, seller, true, false));
        }

        [Fact]
        public void Constructor_WithNullSeller_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(10m), CreateMoney(5m), 1, 0, LotType.Simple, LotStage.Created, seller: null!, true, false));
        }

        [Fact]
        public void Constructor_WithPriceLowerThanCompensation_ThrowsArgumentException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(50m), CreateMoney(60m), 1, 0, LotType.Simple, LotStage.Created, seller, true, false));
        }

        private static Money CreateMoney(decimal amount) => new(amount, Currency.USD);

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller name", "Seller description", "seller@example.com", "+12345678901", null, Guid.NewGuid());
    }
}
