namespace Marketplace
{
    // TODO: Move Auth Dtos to other project?
    public record AuthResponse(
        string AccessToken,
        string RefreshToken,
        bool? IsLockedOut = null,
        bool? RequiresEmailConfirmation = null
        );
}
