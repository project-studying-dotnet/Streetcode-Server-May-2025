using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;

namespace Streetcode.DAL.Specifications.Partner;

public class PartnersByStreetcodeIdSpec : Specification<Entities.Partners.Partner>
{
    public PartnersByStreetcodeIdSpec(int streetcodeId)
    {
        Query.Where(p => p.Streetcodes.Any(sc => sc.Id == streetcodeId) || p.IsVisibleEverywhere)
             .Include(p => p.PartnerSourceLinks);
    }
}
