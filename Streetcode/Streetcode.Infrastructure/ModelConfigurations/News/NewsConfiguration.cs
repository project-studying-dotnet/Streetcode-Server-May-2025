using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Streetcode.Infrasttructure.ModelConfigurations.News;


public class NewsConfiguration : IEntityTypeConfiguration<Domain.Models.News.News>
{
    public void Configure(EntityTypeBuilder<Domain.Models.News.News> builder)
    {
        builder.ToTable("news", schema: "news");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedOnAdd();

        builder.HasIndex(n => n.URL).IsUnique();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(n => n.Text)
            .IsRequired();

        builder.Property(n => n.URL)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.CreationDate)
            .IsRequired();

        builder.HasOne(n => n.Image)
            .WithOne(i => i.News)
            .HasForeignKey<Domain.Models.News.News>(n => n.ImageId);
    }
}
