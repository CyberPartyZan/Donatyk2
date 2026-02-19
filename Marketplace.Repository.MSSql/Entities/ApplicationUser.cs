using Microsoft.AspNetCore.Identity;

namespace Marketplace.Repository.MSSql
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        // TODO: Think about additional properties for the user
        public DateTime CreatedAt { get; set; }
        // TODO: HINT: WARNING: Only for debug purposes. Do not use this field on prod.
        public string Password { get; set; }
    }
}
