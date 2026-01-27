namespace Donatyk2.Server.Dto
{
    public class RegisterUserRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
