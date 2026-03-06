namespace Marketplace
{
    public class Lot
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Money Price { get; set; }
        /// <summary>
        /// Describes how much the seller will get after the lot is sold.
        /// </summary>
        public Money Compensation { get; set; }
        public int StockCount { get; set; }
        public Money? DiscountedPrice { get; set; }
        public double Discount
        {
            get
            {
                if (Price.Amount <= 0)
                {
                    return 0d;
                }

                var discountedAmount = DiscountedPrice?.Amount ?? Price.Amount;
                if (discountedAmount >= Price.Amount)
                {
                    return 0d;
                }

                var discountAmount = Price.Amount - discountedAmount;
                return (double)(discountAmount / Price.Amount * 100m);
            }
        }
        public LotType Type { get; set; }
        public LotStage Stage { get; set; }
        public Seller Seller { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompensationPaid { get; set; }
        public string? DeclineReason { get; set; }
        public Category? Category { get; set; }
        public Money Profit => Price - Compensation;

        public Lot(
            Guid id,
            string name,
            string description,
            Money price,
            Money compensation,
            int stockCount,
            Money? discountedPrice,
            LotType type,
            LotStage stage,
            Seller seller,
            bool isActive,
            bool isCompensationPaid,
            string? declineReason = null,
            Category? category = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Lot name cannot be null or whitespace.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Lot description cannot be null or whitespace.", nameof(description));
            }

            if (price.Amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
            }

            if (compensation.Amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(compensation), "Compensation cannot be negative.");
            }

            if (stockCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stockCount), "Stock count cannot be negative.");
            }

            if (discountedPrice is not null)
            {
                if (discountedPrice.Amount < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(discountedPrice), "Discounted price cannot be negative.");
                }

                if (discountedPrice.Currency != price.Currency)
                {
                    throw new ArgumentException("Discounted price currency must match price currency.", nameof(discountedPrice));
                }

                if (discountedPrice > price)
                {
                    throw new ArgumentOutOfRangeException(nameof(discountedPrice), "Discounted price cannot exceed price.");
                }
            }

            if (seller == null)
            {
                throw new ArgumentNullException(nameof(seller), "Seller cannot be null.");
            }

            if (price < compensation)
            {
                throw new ArgumentException("Price cannot be less than Compensation.");
            }

            Id = id;
            Name = name;
            Description = description;
            // TODO: Consider adding currency support in the future.
            // TODO: Consider adding sets or bundles price in the future.
            // TODO: Price can't be less than Compensation.
            Price = price;
            Compensation = compensation;
            StockCount = stockCount;
            DiscountedPrice = discountedPrice;
            Type = type;
            Stage = stage;
            Seller = seller;
            IsActive = isActive;
            IsCompensationPaid = isCompensationPaid;
            DeclineReason = declineReason;
            Category = category;
        }
    }
}
