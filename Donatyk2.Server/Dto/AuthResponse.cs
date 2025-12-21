namespace Donatyk2.Server.Dto
{
    public record AuthResponse(
        string? AccessToken,
        string? RefreshToken
        );
}
