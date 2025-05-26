using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Streetcode.DAL.Entities.Toponyms;

namespace Streetcode.DAL.Configurations.Toponyms;

public class ToponymConfiguration : IEntityTypeConfiguration<Toponym>
{
    public void Configure(EntityTypeBuilder<Toponym> builder)
    {
        builder
            .HasOne(d => d.Coordinate)
            .WithOne(p => p.Toponym)
            .OnDelete(DeleteBehavior.Cascade);
    }
}