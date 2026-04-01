namespace Marketplace
{
    public sealed class ResendEmailConfirmationRequest
    {
        public string? Email { get; set; }
        public string? RedirectUrl { get; set; }
    }
}