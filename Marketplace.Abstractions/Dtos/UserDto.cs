namespace Donatyk2.Server.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
        public string UserName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }
}
