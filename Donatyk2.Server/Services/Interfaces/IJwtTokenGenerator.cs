using Donatyk2.Server.Data;

namespace Donatyk2.Server.Services.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateAsync(ApplicationUser user);
}