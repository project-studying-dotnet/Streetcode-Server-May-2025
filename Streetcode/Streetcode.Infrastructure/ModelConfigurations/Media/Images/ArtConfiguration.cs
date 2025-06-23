using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Streetcode.Domain.Models.Media.Images;

namespace Streetcode.Infrasttructure.ModelConfigurations.Media.Images;

public class ArtConfiguration : IEntityTypeConfiguration<Art>
{
    public void Configure(EntityTypeBuilder<Art> builder)
    {
        builder.ToTable("arts", schema: "media");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.Description)
            .HasMaxLength(400);

        builder.Property(a => a.Title)
            .HasMaxLength(150);

        builder.Property(a => a.ImageId)
            .IsRequired();

        builder.HasOne(a => a.Image)
            .WithOne(i => i.Art)
            .HasForeignKey<Art>(a => a.ImageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.StreetcodeArts)
            .WithOne(sa => sa.Art)
            .HasForeignKey(sa => sa.ArtId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}