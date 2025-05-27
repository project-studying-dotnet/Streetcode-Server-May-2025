using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;

namespace Streetcode.DAL.Configurations.AdditionalContent.Coordinates;

public class CoordinateConfiguration : IEntityTypeConfiguration<Coordinate>
{
    public void Configure(EntityTypeBuilder<Coordinate> builder)
    {
        builder
            .HasDiscriminator<string>("CoordinateType")
            .HasValue<Coordinate>("coordinate_base")
            .HasValue<StreetcodeCoordinate>("coordinate_streetcode")
            .HasValue<ToponymCoordinate>("coordinate_toponym");
    }
}