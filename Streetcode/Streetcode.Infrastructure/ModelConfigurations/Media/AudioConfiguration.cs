using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Streetcode.Domain.Models.Media;

namespace Streetcode.Infrasttructure.ModelConfigurations.Media;

public class AudioConfiguration : IEntityTypeConfiguration<Audio>
{
    public void Configure(EntityTypeBuilder<Audio> builder)
    {
        builder.ToTable("audios", schema: "media");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.Title)
            .HasMaxLength(100);

        builder.Property(a => a.BlobName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.MimeType)
            .IsRequired()
            .HasMaxLength(10);

        builder.Ignore(a => a.Base64);

        builder.HasOne(a => a.Streetcode)
            .WithOne(sc => sc.Audio)
            .HasForeignKey<StreetcodeContent>(sc => sc.AudioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}