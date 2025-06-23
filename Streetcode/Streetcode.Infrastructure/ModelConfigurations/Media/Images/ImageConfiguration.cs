using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Streetcode.Domain.Models.Media.Images;

namespace Streetcode.Infrasttructure.ModelConfigurations.Media.Images;

public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.ToTable("images", schema: "media");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.BlobName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.MimeType)
            .IsRequired()
            .HasMaxLength(10);

        builder.Ignore(i => i.Base64); 

        builder
            .HasOne(i => i.Art)
            .WithOne(a => a.Image)
            .HasForeignKey<Art>(a => a.ImageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(i => i.ImageDetails)
            .WithOne(d => d.Image)
            .HasForeignKey<ImageDetails>(d => d.ImageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(i => i.Partner)
            .WithOne(p => p.Logo)
            .HasForeignKey<Partner>(p => p.LogoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(i => i.Facts)
            .WithOne(f => f.Image)
            .HasForeignKey(f => f.ImageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(i => i.SourceLinkCategories)
            .WithOne(s => s.Image)
            .HasForeignKey(s => s.ImageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}