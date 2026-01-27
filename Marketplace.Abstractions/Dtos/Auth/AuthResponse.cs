namespace Donatyk2.Server.Dto
{
    public record AuthResponse(
        string AccessToken,
        string RefreshToken,
        bool? IsLockedOut = null,
        bool? RequiresEmailConfirmation = null
        );
}
