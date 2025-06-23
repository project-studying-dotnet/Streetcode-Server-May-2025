using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Streetcode.Domain.Models.Media.Images;

namespace Streetcode.Infrasttructure.ModelConfigurations.Media.Images;

public class ImageDetailsConfiguration : IEntityTypeConfiguration<ImageDetails>
{
    public void Configure(EntityTypeBuilder<ImageDetails> builder)
    {
        builder.ToTable("image_details", schema: "media");

        builder.HasKey(id => id.Id);
        builder.Property(id => id.Id).ValueGeneratedOnAdd();

        builder.Property(id => id.Title)
            .HasMaxLength(100);

        builder.Property(id => id.Alt)
            .HasMaxLength(300);

        builder.Property(id => id.ImageId)
            .IsRequired();

        builder.HasOne(id => id.Image)
            .WithOne(i => i.ImageDetails)
            .HasForeignKey<ImageDetails>(id => id.ImageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}