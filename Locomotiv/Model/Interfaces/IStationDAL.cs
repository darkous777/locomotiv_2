using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.Interfaces
{
    public interface IStationDAL
    {
        Station? FindById(int id);
        IList<Station> GetAll();
        IList<Train> GetAllTrain();
        IList<Train> GetTrainsForStation(int stationId);
        IList<Train> GetTrainsInStation(int stationId);
        void RemoveTrainFromStation(int stationId, int trainId);
        void RemoveTrainFromAllStations(int trainId);
        void AddTrainToStation(int stationId, int trainId, bool addToTrainsInStation);

        void CreateTrainForStation(int stationId, Train train);
        void DeleteTrainPermanently(int stationId, int trainId);
        IList<PredefinedRoute> PrefefinedRouteForEachTrain();
        IList<Block> GetBlocksForPredefinedRoute(List<int> idBlocks);


    }
}
