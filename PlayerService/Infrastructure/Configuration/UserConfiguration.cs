using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.Property(b => b.Money)
            .HasPrecision(18, 2);
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.TwoFactorEnabled);

        builder.HasMany(UserRole => UserRole.Roles)
            .WithOne(user => user.User)
            .HasForeignKey(user => user.UserId)
            .IsRequired();

    }
}
