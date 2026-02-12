using Microsoft.AspNetCore.Identity;

namespace Donatyk2.Server.Data
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        // TODO: Think about additional properties for the user
        public DateTime CreatedAt { get; set; }
        // TODO: HINT: WARNING: Only for debug purposes. Do not use this field on prod.
        public string Password { get; set; }
    }
}
