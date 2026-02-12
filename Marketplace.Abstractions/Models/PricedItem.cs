using Donatyk2.Server.ValueObjects;

namespace Donatyk2.Server.Models
{
    public class PricedItem
    {
        public Guid LotId { get; }
        public string Name { get; }
        public Money BasePrice { get; }
        public double DiscountPercent { get; }
        public decimal TaxRate { get; }
        public int Quantity { get; }
        public Money DiscountPerUnit { get; }
        public Money TaxPerUnit { get; }
        public Money UnitPrice { get; }
        public Money Total => new(UnitPrice.Amount * Quantity, UnitPrice.Currency);

        private PricedItem(
            Guid productId,
            string name,
            Money basePrice,
            double discountPercent,
            decimal taxRate,
            int quantity,
            Money discountPerUnit,
            Money taxPerUnit,
            Money unitPrice)
        {
            LotId = productId;
            Name = name;
            BasePrice = basePrice;
            DiscountPercent = discountPercent;
            TaxRate = taxRate;
            Quantity = quantity;
            DiscountPerUnit = discountPerUnit;
            TaxPerUnit = taxPerUnit;
            UnitPrice = unitPrice;
        }

        public static PricedItem FromLot(Lot lot, int quantity, decimal taxRate)
        {
            if (lot is null)
            {
                throw new ArgumentNullException(nameof(lot));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            }

            if (taxRate < 0 || taxRate > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(taxRate), "Tax rate must be between 0 and 1.");
            }

            var discountAmount = lot.Price.Amount * (decimal)(lot.Discount / 100d);
            var discountedUnitAmount = lot.Price.Amount - discountAmount;
            var taxAmount = discountedUnitAmount * taxRate;
            var finalUnitAmount = discountedUnitAmount + taxAmount;

            Money RoundMoney(decimal amount) => new(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), lot.Price.Currency);

            var discountMoney = RoundMoney(discountAmount);
            var taxMoney = RoundMoney(taxAmount);
            var unitMoney = RoundMoney(finalUnitAmount);

            return new PricedItem(
                lot.Id,
                lot.Name,
                lot.Price,
                lot.Discount,
                taxRate,
                quantity,
                discountMoney,
                taxMoney,
                unitMoney);
        }
    }
}