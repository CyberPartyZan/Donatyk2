namespace Marketplace
{
    public class LoginWithRecoveryCodeRequest
    {
        public string? Email { get; set; }
        public string? RecoveryCode { get; set; }
    }
}