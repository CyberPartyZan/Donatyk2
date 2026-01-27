namespace Donatyk2.Server.Dto
{
    public sealed class ConfirmEmailChangeRequest
    {
        public string? UserId { get; set; }
        public string? NewEmail { get; set; }
        public string? Token { get; set; }
    }
}