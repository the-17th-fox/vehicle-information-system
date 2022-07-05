using Microsoft.AspNetCore.Identity;

namespace AccountsService.Models
{
    public class User : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}
