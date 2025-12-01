using Locomotiv.Model;
using Locomotiv.Utils.Services.Interfaces;

namespace Locomotiv.Utils.Services
{
    public class StationContextService : IStationContextService
    {
        public Station? CurrentStation { get; set; }
    }
}
