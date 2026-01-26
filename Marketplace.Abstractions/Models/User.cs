namespace Marketplace.Abstractions.Models
{
    public sealed class User
    {
        private User() { }

        public User(Guid id, string email, bool emailConfirmed, bool lockoutEnabled, DateTimeOffset? lockoutEnd)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id must be a valid GUID.", nameof(id));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));

            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant))
                throw new ArgumentException("Email is not a valid email address.", nameof(email));

            Id = id;
            Email = email;
            EmailConfirmed = emailConfirmed;
            LockoutEnabled = lockoutEnabled;
            LockoutEnd = lockoutEnd;
        }

        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }
}
