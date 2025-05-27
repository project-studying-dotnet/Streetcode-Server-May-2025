using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;

namespace Streetcode.DAL.Specifications.Video;

public class VideoByIdSpec : Specification<Entities.Media.Video>
{
    public VideoByIdSpec(int id)
    {
        Query.Where(v => v.Id == id);
    }
}
