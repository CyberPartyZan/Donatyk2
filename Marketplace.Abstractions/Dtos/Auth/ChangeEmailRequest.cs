namespace Donatyk2.Server.Dto
{
    public sealed class ChangeEmailRequest
    {
        public string? NewEmail { get; set; }
        public string? Password { get; set; }
        public string? RedirectUrl { get; set; }
    }
}