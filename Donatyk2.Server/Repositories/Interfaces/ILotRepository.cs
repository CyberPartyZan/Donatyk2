using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Models;

namespace Donatyk2.Server.Repositories.Interfaces
{
    public interface ILotRepository
    {
        IEnumerable<Lot> SearchLots(LotSearchQuery query);
        LotEntity GetLotById(Guid id);
        Guid CreateLot(LotEntity lot);
        void UpdateLot(Guid id, LotEntity lot);
        void DeleteLot(Guid id);
    }
}
