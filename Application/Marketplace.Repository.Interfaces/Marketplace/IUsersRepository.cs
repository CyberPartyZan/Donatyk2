namespace Marketplace.Repository
{
    public interface IUsersRepository
    {
        Task<IEnumerable<User>> GetAll(string? search, int page, int pageSize);
        Task<int> GetTotalCount(string? search);
        Task<User?> GetById(Guid id);
        Task<User?> GetByEmail(string email);
        Task Update(User user);
    }
}
