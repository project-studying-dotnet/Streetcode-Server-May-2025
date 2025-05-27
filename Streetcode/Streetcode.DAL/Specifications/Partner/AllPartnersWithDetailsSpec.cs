using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;

namespace Streetcode.DAL.Specifications.Partner;

public class AllPartnersWithDetailsSpec : Specification<Entities.Partners.Partner>
{
    public AllPartnersWithDetailsSpec()
    {
        Query.Include(p => p.PartnerSourceLinks)
             .Include(p => p.Streetcodes);
    }
}
