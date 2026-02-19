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
        public double Discount { get; set; }
        public LotType Type { get; set; }
        public LotStage Stage { get; set; }
        public Seller Seller { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompensationPaid { get; set; }
        public string? DeclineReason { get; set; }
        public Guid CategoryId { get; set; }
        public Money Profit => Price - Compensation;

        public Lot(
            Guid id,
            string name,
            string description,
            Money price,
            Money compensation,
            int stockCount,
            double discount,
            LotType type,
            LotStage stage,
            Seller seller,
            bool isActive,
            bool isCompensationPaid,
            string? declineReason = null)
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

            if (discount < 0 || discount > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(discount), "Discount must be between 0 and 100.");
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
            Discount = discount;
            Type = type;
            Stage = stage;
            Seller = seller;
            IsActive = isActive;
            IsCompensationPaid = isCompensationPaid;
            DeclineReason = declineReason;
        }
    }
}
