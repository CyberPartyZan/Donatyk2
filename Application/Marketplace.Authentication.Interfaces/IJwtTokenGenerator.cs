namespace Marketplace.Authentication;

public interface IJwtTokenGenerator
{
    Task<string> GenerateAsync(Guid userId);
}