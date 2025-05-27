using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Streetcode.DAL.Specifications.Video;

public class VideoByStreetcodeIdSpec : Specification<Entities.Media.Video>
{
    public VideoByStreetcodeIdSpec(int streetcodeId)
    {
        Query.Where(v => v.StreetcodeId == streetcodeId);
    }
}
