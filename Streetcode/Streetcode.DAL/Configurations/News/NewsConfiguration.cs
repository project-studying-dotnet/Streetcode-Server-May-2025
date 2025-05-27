using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Entity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.DAL.Configurations.News;

public class NewsConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder
            .HasOne(x => x.Image)
            .WithOne(x => x.News)
            .HasForeignKey<Entity>(x => x.ImageId);
    }
}