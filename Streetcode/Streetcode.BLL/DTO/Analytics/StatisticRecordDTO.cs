using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.DTO.Streetcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.DTO.Analytics;

public class StatisticRecordDTO
{
    public int Id { get; set; }
    public int QrId { get; set; }
    public int Count { get; set; }
    public string Address { get; set; }
    public int StreetcodeId { get; set; }
    public StreetcodeContentDTO? Streetcode { get; set; }
    public int StreetcodeCoordinateId { get; set; }
    public StreetcodeCoordinateDTO StreetcodeCoordinate { get; set; }
}
