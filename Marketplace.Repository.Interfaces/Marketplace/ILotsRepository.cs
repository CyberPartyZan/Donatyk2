using Donatyk2.Server.Dto;
using Donatyk2.Server.Models;

namespace Donatyk2.Server.Repositories.Interfaces
{
    public interface ILotsRepository
    {
        Task<IEnumerable<Lot>> GetAll(LotSearchQuery query);
        Task<Lot?> GetLotById(Guid id);
        Task<Guid> CreateLot(Lot lot);
        Task UpdateLot(Guid id, Lot lot);
        Task DeleteLot(Guid id);
        Task ApproveLot(Guid id);
    }
}
