using Marketplace.Abstractions.Models;

namespace Donatyk2.Server.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task<IEnumerable<User>> GetAll(string? search, int page, int pageSize);
        Task<User?> GetById(Guid id);
        Task<User?> GetByEmail(string email);
        Task Update(User user);
        Task Delete(Guid id);
    }
}
