using Donatyk2.Server.Dto;
using Donatyk2.Server.Models;

namespace Donatyk2.Server.Services
{
    public interface ILotsService
    {
        IEnumerable<LotDto> SearchLots(LotSearchQuery query);
        LotDto GetLotById(Guid id);
        Guid CreateLot(LotDto lot);
        void UpdateLot(Guid id, LotDto lot);
        void DeleteLot(Guid id);
    }
}
