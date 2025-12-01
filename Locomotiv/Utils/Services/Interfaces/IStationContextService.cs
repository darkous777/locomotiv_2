using Locomotiv.Model;

namespace Locomotiv.Utils.Services.Interfaces
{
    public interface IStationContextService
    {
        Station? CurrentStation { get; set; }
    }
}
