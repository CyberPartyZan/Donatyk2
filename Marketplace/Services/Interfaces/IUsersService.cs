namespace Marketplace
{
    public interface IUsersService
    {
        Task<IEnumerable<UserDto>> GetAll(string? search, int page, int pageSize);
        Task<UserDto?> GetById(Guid id);
        Task<UserDto?> GetByEmail(string email);
        Task Update(UserDto user);
    }
}
