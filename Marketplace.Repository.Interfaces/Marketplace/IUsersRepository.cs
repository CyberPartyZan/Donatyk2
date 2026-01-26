using Donatyk2.Server.Data;

namespace Donatyk2.Server.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAll(string? search, int page, int pageSize);
        Task<ApplicationUser?> GetById(Guid id);
        Task<ApplicationUser?> GetByEmail(string email);
        Task<Guid> Create(ApplicationUser user);
        Task Update(ApplicationUser user);
        Task Delete(Guid id);
    }
}
