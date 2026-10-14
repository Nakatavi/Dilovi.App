using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Patronymic).HasMaxLength(100);
        builder.Property(u => u.PhoneNumber).HasMaxLength(32);
        builder.Property(u => u.PasswordHash).HasMaxLength(100).IsRequired();

        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(32);
    }
}
