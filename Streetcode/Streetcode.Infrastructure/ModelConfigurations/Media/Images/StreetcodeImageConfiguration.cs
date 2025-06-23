using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Streetcode.Domain.Models.Media.Images;

namespace Streetcode.Infrasttructure.ModelConfigurations.Media.Images;

public class StreetcodeImageConfiguration : IEntityTypeConfiguration<StreetcodeImage>
{
    public void Configure(EntityTypeBuilder<StreetcodeImage> builder)
    {
        builder.HasKey(si => new { si.StreetcodeId, si.ImageId });
        
        builder.Property(si => si.StreetcodeId).IsRequired();
        builder.Property(si => si.ImageId).IsRequired();

        builder.HasOne(si => si.Image)
            .WithMany()
            .HasForeignKey(si => si.ImageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.Streetcode)
            .WithMany()
            .HasForeignKey(si => si.StreetcodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}