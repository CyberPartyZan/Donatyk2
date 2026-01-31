using System;

namespace Donatyk2.Server.Services.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateAsync(Guid userId);
}