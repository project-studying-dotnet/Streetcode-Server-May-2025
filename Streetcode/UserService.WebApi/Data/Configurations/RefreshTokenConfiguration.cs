using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.WebApi.Entities.Auth;

namespace UserService.WebApi.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder
            .HasKey(r => r.Id);

        builder
            .Property(r => r.Token)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .HasIndex(r => r.Token)
            .IsUnique();

        builder
            .Property(r => r.ExpiresAt)
            .IsRequired();

        builder
            .Property(r => r.IsRevoked)
            .HasDefaultValue(false);

        builder
            .Property(r => r.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}