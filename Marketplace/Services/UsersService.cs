using Donatyk2.Server.Dto;
using Donatyk2.Server.Repositories.Interfaces;
using Donatyk2.Server.Services.Interfaces;
using Marketplace.Abstractions.Models;

namespace Donatyk2.Server.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ISellersService _sellersService;

        public UsersService(IUsersRepository usersRepository, ISellersService sellersService)
        {
            _usersRepository = usersRepository;
            _sellersService = sellersService;
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

        public async Task Delete(Guid id)
        {
            await _usersRepository.Delete(id);
            await _sellersService.DeleteByUserId(id);
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