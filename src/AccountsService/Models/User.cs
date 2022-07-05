using Microsoft.AspNetCore.Identity;

namespace AccountsService.Models
{
    public class User : IdentityUser<Guid>
    {
        public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; private set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}
