using Donatyk2.Server.Data;
using Donatyk2.Server.Enums;

namespace Donatyk2.Server.Models
{
    public class Lot
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        /// <summary>
        /// Describes how much the seller will get after the lot is sold.
        /// </summary>
        public double Compensation { get; set; }
        public LotType Type { get; set; }
        public Seller Seller { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompensationPaid { get; set; }
        public double Profit => Price - Compensation;

        public Lot(LotEntity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Description = entity.Description;
            // TODO: Consider adding discounts or dynamic pricing in the future.
            // TODO: Consider adding currency support in the future.
            // TODO: Consider adding tax calculations in the future.
            // TODO: Consider adding regional pricing in the future.
            // TODO: Consider adding promotional pricing in the future.
            // TODO: Consider adding sets or bundles price in the future.
            // TODO: Price can't be less than Compensation.
            Price = entity.Price;
            Compensation = entity.Compensation;
            Type = entity.Type;
            Seller = new Seller(entity.Seller);
            IsActive = entity.IsActive;
            IsCompensationPaid = entity.IsCompensationPaid;
        }

    }
}
