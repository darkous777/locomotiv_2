using System.Collections.Generic;
using System.Linq;
using Locomotiv.Model;
using Locomotiv.Model.Interfaces;

namespace Locomotiv.Utils.Services.Map
{
    public class MapInfoService
    {
        private readonly IBlockDAL _blockDal;

        public MapInfoService(IBlockDAL blockDal)
        {
            _blockDal = blockDal;
        }

        public string GetStationInfo(Station station)
        {
            string header = $"🏢 Station : {station.Name}\n📍 Localisation : ({station.Latitude}, {station.Longitude})";

            string assignedTrains = FormatTrainList(station.Trains, "Aucun train attribué");
            string trainsInStation = FormatTrainList(station.TrainsInStation, "Aucun train actuellement en gare");
            string signals = "   Aucun signal enregistré";

            return $"{header}\n\n" +
                   $"🚆 Trains attribués :\n{assignedTrains}\n\n" +
                   $"🚉 Trains en gare :\n{trainsInStation}\n\n" +
                   $"🚦 Signaux :\n{signals}";
        }

        public string GetBlockPointInfo(BlockPoint blockPoint)
        {
            IList<Block> connectedBlocks = _blockDal.GetBlocksByPointId(blockPoint.Id);

            List<string> blockDescriptions = connectedBlocks.Select(block =>
            {
                BlockPoint otherPoint = block.Points.FirstOrDefault(p => p.Id != blockPoint.Id);
                string status = block.CurrentTrain != null ? "Train présent" : "Libre";
                string destination = otherPoint != null
                    ? $"vers BlockPoint {otherPoint.Id}"
                    : "(point unique)";

                return $" - Block {block.Id} ({status}) → {destination}";
            }).ToList();

            string blocksInfo = string.Join("\n", blockDescriptions);
            return $"🛤️ BlockPoint {blockPoint.Id}\n\nBlocs connectés :\n{blocksInfo}";
        }

        public string GetTrainInfo(Train train)
        {
            return $"🚆 Train {train.Id}\n" +
                   $"Type: {train.TypeOfTrain}\n" +
                   $"Priorité: {train.PriotityLevel}\n" +
                   $"État: {train.State}";
        }

        private string FormatTrainList(ICollection<Train> trains, string emptyMessage)
        {
            return trains != null && trains.Count > 0
                ? string.Join("\n", trains.Select(t => $"   • 🚉 Train {t.Id}"))
                : $"   {emptyMessage}";
        }
    }
}
