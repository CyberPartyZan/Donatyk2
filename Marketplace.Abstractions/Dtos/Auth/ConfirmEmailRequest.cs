namespace Donatyk2.Server.Dto
{
    public sealed class ConfirmEmailRequest
    {
        public string? UserId { get; set; }
        public string? Token { get; set; }
    }
}