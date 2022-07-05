using AccountsService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AccountsService.Infrastructure.Context
{
    public class AccountsServiceContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AccountsServiceContext(DbContextOptions<AccountsServiceContext> opt) : base(opt) 
        {
            Database.EnsureCreated();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e =>
                    e.State == EntityState.Modified || e.State == EntityState.Added)
                .Where(e =>
                    e.Properties.Where(p =>
                        p.Metadata.Name == "UpdatedAt" || p.Metadata.Name == "CreatedAt").Any());

            if (!entries.Any())
                return await base.SaveChangesAsync(cancellationToken);

            foreach (var entityEntry in entries)
            {
                switch (entityEntry.State)
                {
                    case EntityState.Modified:
                        entityEntry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                        break;

                    case EntityState.Added:
                        entityEntry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
