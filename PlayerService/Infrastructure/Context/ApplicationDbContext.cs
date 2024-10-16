using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Context;


public class ApplicationDbContext : IdentityDbContext<User, Role, Guid
    , IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>
    , IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {

        base.OnModelCreating(builder);
        builder.SeedUsers();
        builder.SeedRoles();
        builder.SeedUserRoles();

        builder.ApplyConfigurationsFromAssembly(
            typeof(Configuration.UserConfiguration).Assembly
        );


    }
}
