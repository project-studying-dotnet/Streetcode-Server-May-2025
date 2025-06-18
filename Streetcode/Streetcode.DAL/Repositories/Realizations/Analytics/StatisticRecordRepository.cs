using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Analytics;
using Streetcode.DAL.Repositories.Realizations.Base;

namespace Streetcode.DAL.Repositories.Realizations.Analytics;

public class StatisticRecordRepository : RepositoryBase<StatisticRecord>, IStatisticRecordRepository
{
    public StatisticRecordRepository(StreetcodeDbContext context)
        : base(context)
    {
    }
}