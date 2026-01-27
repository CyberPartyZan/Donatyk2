namespace Donatyk2.Server.Dto
{
    public sealed class ResendEmailConfirmationRequest
    {
        public string? Email { get; set; }
        public string? RedirectUrl { get; set; }
    }
}