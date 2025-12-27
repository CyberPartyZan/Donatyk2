using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Repositories.Interfaces;
using Donatyk2.Server.Services.Interfaces;
using System.Security.Claims;

namespace Donatyk2.Server.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ClaimsPrincipal _user;

        public UsersService(ClaimsPrincipal user, IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
            _user = user;
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
            existing.UserName = userDto.UserName;
            existing.PhoneNumber = userDto.PhoneNumber;
            existing.EmailConfirmed = userDto.EmailConfirmed;
            existing.LockoutEnabled = userDto.LockoutEnabled;
            existing.LockoutEnd = userDto.LockoutEnd ?? existing.LockoutEnd;

            await _usersRepository.Update(existing);
        }

        public async Task Delete(Guid id)
        {
            await _usersRepository.Delete(id);
        }

        private static UserDto ToDto(ApplicationUser u)
        {
            return new UserDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                EmailConfirmed = u.EmailConfirmed,
                UserName = u.UserName ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                LockoutEnabled = u.LockoutEnabled,
                LockoutEnd = u.LockoutEnd?.UtcDateTime
            };
        }
    }
}