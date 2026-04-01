using Marketplace.Repository;

namespace Marketplace
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _usersRepository;

        public UsersService(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<IEnumerable<UserDto>> GetAll(string? search, int page, int pageSize)
        {
            var users = await _usersRepository.GetAll(search, page, pageSize);
            return users.Select(ToDto);
        }

        public async Task<UserDto?> GetById(Guid id)
        {
            var user = await _usersRepository.GetById(id);
            return user is null ? null : ToDto(user);
        }

        public async Task<UserDto?> GetByEmail(string email)
        {
            var user = await _usersRepository.GetByEmail(email);
            return user is null ? null : ToDto(user);
        }

        public async Task Update(UserDto userDto)
        {
            var existing = await _usersRepository.GetById(userDto.Id);
            if (existing is null)
            {
                throw new KeyNotFoundException($"User with id '{userDto.Id}' not found.");
            }

            existing.Email = userDto.Email;
            existing.EmailConfirmed = userDto.EmailConfirmed;
            existing.LockoutEnabled = userDto.LockoutEnabled;
            existing.LockoutEnd = userDto.LockoutEnd ?? existing.LockoutEnd;

            await _usersRepository.Update(existing);
        }

        private static UserDto ToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd
            };
        }
    }
}