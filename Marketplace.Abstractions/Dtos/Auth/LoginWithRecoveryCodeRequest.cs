namespace Donatyk2.Server.Dto
{
    public class LoginWithRecoveryCodeRequest
    {
        public string? Email { get; set; }
        public string? RecoveryCode { get; set; }
    }
}