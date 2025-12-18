using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.Utils.Services.Map;
using Locomotiv.ViewModel;
using Moq;

namespace LocomotivTests.ViewModel
{
    public class MapViewModelTest
    {
        private readonly Mock<IStationDAL> _stationDALMock;
        private readonly Mock<IBlockPointDAL> _blockPointsDALMock;
        private readonly Mock<IBlockDAL> _blockDALMock;
        private readonly Mock<ILoggingService> _logsServicesMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IStationContextService> _stationContextServiceMock;
        private readonly Mock<IUserSessionService> _userSessionServiceMock;
        private readonly Mock<IPredefinedRouteDAL> _predefinedRouteDALMock;
        private readonly Mock<ITrainDAL> _trainDALMock;
        private readonly Mock<TrainMovementService> _trainMovementServiceMock;
        private readonly Mock<MapMarkerFactory> _markerFactoryMock;
        private readonly Mock<MapInfoService> _infoServiceMock;
        private readonly MapViewModel _viewmodel;
        private readonly Station _station;
        private readonly Station _emptyStation;
        private readonly Train _trainInTestStation;
        private readonly Train _trainNotInTestStation;
        private readonly List<BlockPoint> _blockPoints;
        private readonly Block _block;
        private readonly Block _blockNotConnected;

        public MapViewModelTest()
        {
            _stationDALMock = new Mock<IStationDAL>();
            _blockPointsDALMock = new Mock<IBlockPointDAL>();
            _blockDALMock = new Mock<IBlockDAL>();
            _navigationServiceMock = new Mock<INavigationService>();
            _stationContextServiceMock = new Mock<IStationContextService>();
            _userSessionServiceMock = new Mock<IUserSessionService>();
            _predefinedRouteDALMock = new Mock<IPredefinedRouteDAL>();
            _trainDALMock = new Mock<ITrainDAL>();
            _trainMovementServiceMock = new Mock<TrainMovementService>(_stationDALMock.Object, _blockDALMock.Object, _logsServicesMock.Object);
            _markerFactoryMock = new Mock<MapMarkerFactory>();
            _infoServiceMock = new Mock<MapInfoService>(_blockDALMock.Object);

            _viewmodel = new MapViewModel(
                _stationDALMock.Object,
                _blockDALMock.Object,
                _blockPointsDALMock.Object,
                _predefinedRouteDALMock.Object,
                _trainDALMock.Object,
                _navigationServiceMock.Object,
                _stationContextServiceMock.Object,
                _userSessionServiceMock.Object,
                _trainMovementServiceMock.Object,
                _markerFactoryMock.Object,
                _infoServiceMock.Object,
                false
            );

            _trainInTestStation = new Train
            {
                Id = 1,
                TypeOfTrain = TrainType.Merchandise,
                PriotityLevel = PriorityLevel.Low,
                State = TrainState.Idle,
            };

            _trainNotInTestStation = new Train
            {
                Id = 2,
                TypeOfTrain = TrainType.Passenger,
                PriotityLevel = PriorityLevel.Low,
                State = TrainState.Idle,
            };

            _station = new Station
            {
                Id = 1,
                Name = "Test Station",
                Longitude = -71.204255,
                Latitude = 46.842256,
                Type = StationType.Station,
                TrainsInStation = new List<Train>
                {
                    _trainInTestStation
                },
                Trains = new List<Train>
                {
                    _trainNotInTestStation
                }
            };

            _emptyStation = new Station
            {
                Id = 2,
                Name = "Empty Test Station",
                Longitude = -71.204255,
                Latitude = 46.842256,
                Type = StationType.Station,
                TrainsInStation = new List<Train>() { },
                Trains = new List<Train>() { }
            };

            _blockPoints = new List<BlockPoint>
            {
                new BlockPoint
                {
                    Id = 1,
                    Longitude = -71.204255,
                    Latitude = 46.842256
                },
                new BlockPoint
                {
                    Id = 2,
                    Longitude = -71.334879,
                    Latitude = 46.747842
                },
                new BlockPoint
                {
                    Id = 3,
                    Longitude = -71.123456,
                    Latitude = 46.654321
                }
            };

            _block = new Block
            {
                Id = 1,
                Points = new List<BlockPoint>
                {
                    _blockPoints.First(bp => bp.Id == 1),
                    _blockPoints.First(bp => bp.Id == 2)
                }
            };

            _blockNotConnected = new Block
            {
                Id = 2,
                Points = new List<BlockPoint>
                {
                    _blockPoints.First(bp => bp.Id == 3)
                }
            };

        }

        [Fact]
        public void GetStationInfo_HasTrains_ReturnsValidStationInfoString()
        {
            // Arrange
            Station station = _station;

            // Act
            string stationstring = _viewmodel.GetStationInfo(station);

            // Assert
            Assert.Equal(
                $"🏢 Station : Test Station\n" +
                $"📍 Localisation : ({_station.Latitude}, " +
                $"{_station.Longitude})\n\n" +
                $"🚆 Trains attribués :\n" +
                $"   • 🚉 Train 2\n\n" +
                $"🚉 Trains en gare :\n" +
                $"   • 🚉 Train 1\n\n" +
                $"🚦 Signaux :\n" +
                $"   Aucun signal enregistré",
            stationstring);
        }

        [Fact]
        public void GetStationInfo_HasNoTrains_ReturnsValidStationInfoString()
        {
            // Arrange
            Station station = _emptyStation;

            // Act
            string stationstring = _viewmodel.GetStationInfo(station);

            // Assert
            Assert.Equal(
                $"🏢 Station : Empty Test Station\n" +
                $"📍 Localisation : ({_emptyStation.Latitude}, " +
                $"{_emptyStation.Longitude})\n\n" +
                $"🚆 Trains attribués :\n" +
                $"   Aucun train attribué\n\n" +
                $"🚉 Trains en gare :\n" +
                $"   Aucun train actuellement en gare\n\n" +
                $"🚦 Signaux :\n" +
                $"   Aucun signal enregistré",
            stationstring);
        }

        [Fact]
        public void GetBlockInfo_NoConnectedPoints_ReturnsCorrectBlockInfoString()
        {
            // Arrange
            BlockPoint blockPoint = _blockPoints[0];

            // Act
            _blockDALMock.Setup(d => d.GetBlocksByPointId(1)).Returns(new List<Block> { _block });

            string blockstring = _viewmodel.GetBlockInfo(_blockPoints[0]);

            // Assert
            Assert.Equal(
                $"🛤️ BlockPoint 1\n\n" +
                $"Blocs connectés :\n - Block 1 (Libre) → vers BlockPoint 2",
                blockstring);
        }

        [Fact]
        public void GetBlockInfo_ConnectedPoints_ReturnsCorrectBlockInfoString()
        {
            // Arrange
            BlockPoint blockPoint = _blockPoints[2];

            // Act
            _blockDALMock.Setup(d => d.GetBlocksByPointId(3))
                .Returns(new List<Block> { _blockNotConnected });

            string blockstring = _viewmodel.GetBlockInfo(_blockPoints[2]);

            // Assert
            Assert.Equal(
                $"🛤️ BlockPoint 3\n\n" +
                $"Blocs connectés :\n - Block 2 (Libre) → (point unique)",
                blockstring);
        }
    }
}
