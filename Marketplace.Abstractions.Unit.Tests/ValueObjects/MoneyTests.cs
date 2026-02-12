using System;
using Donatyk2.Server.Enums;
using Donatyk2.Server.ValueObjects;
using Xunit;

namespace Marketplace.Abstractions.Unit.Tests.ValueObjects
{
    public sealed class MoneyTests
    {
        [Fact]
        public void Addition_WithSameCurrency_ReturnsCombinedAmount()
        {
            var first = CreateMoney(12.5m);
            var second = CreateMoney(7.5m);

            var result = first + second;

            Assert.Equal(20m, result.Amount);
            Assert.Equal(first.Currency, result.Currency);
        }

        [Fact]
        public void Addition_WithDifferentCurrency_ThrowsInvalidOperationException()
        {
            var usd = CreateMoney(10m, Currency.USD);
            var eur = CreateMoney(5m, Currency.EUR);

            Assert.Throws<InvalidOperationException>(() => _ = usd + eur);
        }

        [Fact]
        public void Subtraction_WithSameCurrency_ReturnsDifference()
        {
            var minuend = CreateMoney(15m);
            var subtrahend = CreateMoney(4m);

            var result = minuend - subtrahend;

            Assert.Equal(11m, result.Amount);
            Assert.Equal(minuend.Currency, result.Currency);
        }

        [Fact]
        public void UnaryMinus_NegatesAmount()
        {
            var money = CreateMoney(25m);

            var result = -money;

            Assert.Equal(-25m, result.Amount);
            Assert.Equal(money.Currency, result.Currency);
        }

        [Fact]
        public void IncrementOperator_IncreasesAmountByOne()
        {
            var money = CreateMoney(3m);

            var incremented = ++money;

            Assert.Equal(4m, incremented.Amount);
            Assert.Equal(money.Currency, incremented.Currency);
        }

        [Fact]
        public void ComparisonOperators_WithSameCurrency_ReturnExpectedResults()
        {
            var smaller = CreateMoney(5m);
            var larger = CreateMoney(10m);

            Assert.True(smaller < larger);
            Assert.True(larger > smaller);
            Assert.True(smaller <= larger);
            Assert.True(larger >= smaller);
            Assert.True(smaller <= smaller);
            Assert.True(larger >= larger);
        }

        [Fact]
        public void ComparisonOperators_WithDifferentCurrency_ThrowsInvalidOperationException()
        {
            var usd = CreateMoney(5m, Currency.USD);
            var eur = CreateMoney(6m, Currency.EUR);

            Assert.Throws<InvalidOperationException>(() => _ = usd < eur);
            Assert.Throws<InvalidOperationException>(() => _ = usd > eur);
        }

        [Fact]
        public void ComparisonOperators_WithNullOperands_ThrowArgumentNullException()
        {
            var money = CreateMoney(5m);

            Assert.Throws<ArgumentNullException>(() => _ = (Money)null! < money);
            Assert.Throws<ArgumentNullException>(() => _ = money < (Money)null!);
        }

        private static Money CreateMoney(decimal amount, Currency currency = Currency.USD) =>
            new(amount, currency);
    }
}
