namespace Marketplace.Server.Identity
{
    public sealed class AdminUserOptions
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";
        public bool EmailConfirmed { get; set; } = true;

        public bool IsValid() =>
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Password);
    }
}