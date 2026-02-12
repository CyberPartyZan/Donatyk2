using Donatyk2.Server.Dto;

namespace Donatyk2.Server.Services
{
    public interface ILotsService
    {
        Task<IEnumerable<LotDto>> GetAll(LotSearchQuery query);
        Task<LotDto?> GetLotById(Guid id);
        Task<Guid> CreateLot(LotDto lot);
        Task UpdateLot(Guid id, LotDto lot);
        Task DeleteLot(Guid id);
        Task ApproveLot(Guid id);
    }
}
