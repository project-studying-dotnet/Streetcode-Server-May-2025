using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.DTO.Analytics;

public record StatisticRecordCreateDTO
{
    public int? QrId { get; init; }
    public int Count { get; init; }
    public string Address { get; init; }
    public int StreetcodeId { get; init; }
    public int StreetcodeCoordinateId { get; init; }
}
