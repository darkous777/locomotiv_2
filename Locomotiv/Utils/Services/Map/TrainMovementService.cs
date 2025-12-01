using Locomotiv.Model;
using Locomotiv.Model.Interfaces;

namespace Locomotiv.Utils.Services.Map
{
    public class TrainMovementService
    {
        private readonly IStationDAL _stationDal;
        private readonly IBlockDAL _blockDal;

        public TrainMovementService(IStationDAL stationDal, IBlockDAL blockDal)
        {
            _stationDal = stationDal;
            _blockDal = blockDal;
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
            int currentTrainsInStation = station.TrainsInStation?.Count ?? 0;
            bool hasCapacity = currentTrainsInStation < station.Capacity;

            _stationDal.AddTrainToStation(station.Id, train.Id, addToTrainsInStation: hasCapacity);
        }

        public void DepartFromAllStations(int trainId)
        {
            _stationDal.RemoveTrainFromAllStations(trainId);
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
