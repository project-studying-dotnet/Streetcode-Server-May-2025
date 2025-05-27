using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;

namespace Streetcode.DAL.Specifications.Partner;

public class PartnerByIdSpec : Specification<Entities.Partners.Partner>
{
    public PartnerByIdSpec(int id)
    {
        Query.Where(p => p.Id == id)
             .Include(p => p.PartnerSourceLinks);
    }
}
