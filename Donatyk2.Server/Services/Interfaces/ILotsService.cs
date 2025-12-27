using Donatyk2.Server.Dto;
using Donatyk2.Server.Models;

namespace Donatyk2.Server.Services
{
    public interface ILotsService
    {
        IEnumerable<Lot> SearchLots(LotSearchQuery query);
        Lot GetLotById(Guid id);
        Guid CreateLot(Lot lot);
        void UpdateLot(Guid id, Lot lot);
        void DeleteLot(Guid id);
    }
}
