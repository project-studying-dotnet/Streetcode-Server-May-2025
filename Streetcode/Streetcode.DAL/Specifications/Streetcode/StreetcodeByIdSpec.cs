using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Streetcode.DAL.Specifications.Streetcode;

public class StreetcodeByIdSpec : Specification<Entities.Streetcode.StreetcodeContent>
{
    public StreetcodeByIdSpec(int id)
    {
        Query.Where(s => s.Id == id);
    }
}
