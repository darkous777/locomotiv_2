using Locomotiv.Model;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;

namespace Locomotiv.Utils.Services.Map
{
    public class TrainMovementService
    {
        private readonly IStationDAL _stationDal;
        private readonly IBlockDAL _blockDal;
        private readonly ILoggingService _loggingService;

        public TrainMovementService(IStationDAL stationDal, IBlockDAL blockDal, ILoggingService loggingService)
        {
            _stationDal = stationDal;
            _blockDal = blockDal;
            _loggingService = loggingService;
        }

        public void MoveTrainToBlock(Train train, Block currentBlock, Block nextBlock)
        {
            currentBlock.CurrentTrain = null;
            _blockDal.Update(currentBlock);

            train.Latitude = nextBlock.Latitude;
            train.Longitude = nextBlock.Longitude;

            nextBlock.CurrentTrain = train;
            _blockDal.Update(nextBlock);
        }

        public void ArriveAtStation(Train train, Station station)
        {
            try
            {
                int currentTrainsInStation = station.TrainsInStation?.Count ?? 0;
                bool hasCapacity = currentTrainsInStation < station.Capacity;

                _stationDal.AddTrainToStation(station.Id, train.Id, addToTrainsInStation: hasCapacity);

                _loggingService.LogInfo($"Train #{train.Id} est arrivé à la station '{station.Name}'.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Erreur lors de l'arrivée du train #{train.Id} à la station '{station.Name}'.", ex);
                throw;
            }
        }

        public void DepartFromAllStations(int trainId)
        {
            try
            {
                _stationDal.RemoveTrainFromAllStations(trainId);

                _loggingService.LogInfo($"Train #{trainId} a quitté la station.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Erreur lors du départ du train #{trainId}.", ex);
                throw;
            }
        }

        public void PlaceTrainOnBlock(Train train, Block block)
        {
            block.CurrentTrain = train;
            train.Latitude = block.Latitude;
            train.Longitude = block.Longitude;
            _blockDal.Update(block);
        }

        public void ClearTrainFromBlock(Block block)
        {
            block.CurrentTrain = null;
            _blockDal.Update(block);
        }
    }
}
