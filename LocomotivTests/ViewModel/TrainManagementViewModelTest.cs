using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;

namespace LocomotivTests.ViewModel
{
    public class TrainManagementViewModelTest
    {
        private readonly Mock<IStationDAL> _stationDALMock;
        private readonly Mock<IStationContextService> _stationContextServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly TrainManagementViewModel _viewmodel;
        private readonly Station _station;
        private readonly Station _emptyStation;
        private readonly Train _trainInTestStation;
        private readonly Train _trainNotInTestStation;

        public TrainManagementViewModelTest()
        {
            _stationDALMock = new Mock<IStationDAL>();
            _stationContextServiceMock = new Mock<IStationContextService>();
            _navigationServiceMock = new Mock<INavigationService>();

            _viewmodel = new TrainManagementViewModel(
                _stationDALMock.Object,
                _stationContextServiceMock.Object,
                _navigationServiceMock.Object
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
        }

        [Fact]
        public void LoadTrainsForStation_TrainsInStation_LoadsItsTrains()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());

            // Act
            _viewmodel.LoadTrainsForStation();

            // Assert
            Assert.Contains(_trainInTestStation, _viewmodel.Trains);
        }

        [Fact]
        public void LoadTrainsForStation_NoTrainsInStation_LoadsNoTrains()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_emptyStation);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_emptyStation);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_emptyStation.TrainsInStation.ToList());

            // Act
            _viewmodel.LoadTrainsForStation();

            // Assert
            Assert.DoesNotContain(_trainInTestStation, _viewmodel.Trains);
        }

        [Fact]
        public void LoadAvailableTrains_AvailableTrains_LoadsTheTrains()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_station.Trains.ToList());

            // Act
            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            // Assert
            Assert.Contains(_trainNotInTestStation, _viewmodel.AvailableTrains);
        }

        [Fact]
        public void LoadAvailableTrains_NoAvailableTrains_LoadsNoTrains()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_emptyStation);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_emptyStation);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_emptyStation.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_emptyStation.Trains.ToList());

            // Act
            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            // Assert
            Assert.DoesNotContain(_trainNotInTestStation, _viewmodel.AvailableTrains);
        }

        [Fact]
        public void AddTrain_CanAddTrain_AddsTrainToStation()
        {
            // Arrange
            _station.Capacity = 2;

            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_station.Trains.ToList());

            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            _viewmodel.SelectedAvailableTrain = _trainNotInTestStation;

            // Act
            _viewmodel.AddTrainCommand.Execute(null);

            // Assert
            _stationDALMock.Verify(
                d => d.AddTrainToStation(_station.Id, _trainNotInTestStation.Id, true),
                Times.Once
            );

            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(new List<Train>
                {
                    _trainInTestStation,
                    _trainNotInTestStation
                });

            _viewmodel.LoadTrainsForStation();

            Assert.Contains(_trainNotInTestStation, _viewmodel.Trains);
        }

        [Fact]
        public void AddTrain_CannotAddTrain_DoesNotAddTrainToStation()
        {
            // Arrange
            _station.Capacity = 1;

            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(new List<Train>());

            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            _viewmodel.SelectedAvailableTrain = null;

            // Act
            _viewmodel.AddTrainCommand.Execute(null);

            // Assert 
            _stationDALMock.Verify(
                d => d.AddTrainToStation(_station.Id, _trainNotInTestStation.Id, true),
                Times.Never
            );

            Assert.DoesNotContain(_trainNotInTestStation, _viewmodel.Trains);
        }

        [Fact]
        public void DeleteTrain_CanDeleteTrain_RemovesTrainFromStation()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_station.Trains.ToList());

            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            _viewmodel.SelectedTrain = _trainInTestStation;

            // Act
            _viewmodel.DeleteTrainCommand.Execute(null);

            // Assert
            _stationDALMock.Verify(d =>
                d.RemoveTrainFromStation(_station.Id, _trainInTestStation.Id),
                Times.Once
            );

            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(new List<Train>());

            _viewmodel.LoadTrainsForStation();

            Assert.DoesNotContain(_trainInTestStation, _viewmodel.Trains);
        }

        [Fact]
        public void CreateTrain_Anytime_NavigatesToCreateTrainView()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                 .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_station.Trains.ToList());

            // Act
            _viewmodel.NavigateToCreateTrainForStationViewCommand.Execute(null);

            // Assert
            _navigationServiceMock.Verify(
                n => n.NavigateTo<CreateTrainForStationViewModel>(),
                Times.Once
            );
        }

        [Fact]
        public void CanAddTrain_SelectedAvailableTrain_ReturnsTrue()
        {
            // Arrange
            _station.Capacity = 2;

            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_station.Trains.ToList());

            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            _viewmodel.SelectedAvailableTrain = _trainNotInTestStation;

            // Act
            bool canAddTrain = _viewmodel.AddTrainCommand.CanExecute(null);

            // Assert
            Assert.True(canAddTrain);
        }

        [Fact]
        public void CanAddTrain_NoSelectedAvailableTrain_ReturnsFalse()
        {
            // Arrange
            _station.Capacity = 2;

            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_station.Trains.ToList());

            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            _viewmodel.SelectedAvailableTrain = null;

            // Act
            bool canAddTrain = _viewmodel.AddTrainCommand.CanExecute(null);

            // Assert
            Assert.False(canAddTrain);
        }

        [Fact]
        public void CanDeleteTrain_SelectedTrain_ReturnsTrue()
        {
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_station.Trains.ToList());

            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            _viewmodel.SelectedTrain = _trainInTestStation;

            // Act
            bool canDeleteTrain = _viewmodel.DeleteTrainCommand.CanExecute(null);

            // Assert
            Assert.True(canDeleteTrain);
        }

        [Fact]
        public void CanDeleteTrain_NoSelectedAvailableTrain_ReturnsFalse()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_station.Trains.ToList());

            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            _viewmodel.SelectedTrain = null;

            // Act
            bool canDeleteTrain = _viewmodel.DeleteTrainCommand.CanExecute(null);

            // Assert
            Assert.False(canDeleteTrain);
        }

        [Fact]
        public void CanCreateTrain_SelectedStation_ReturnsTrue()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_station.Trains.ToList());

            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            // Act
            bool canCreateTrain = _viewmodel.NavigateToCreateTrainForStationViewCommand
                .CanExecute(null);

            // Assert
            Assert.True(canCreateTrain);
        }

        [Fact]
        public void CanCreateTrain_NoSelectedStation_ReturnsFalse()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList());
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(_station.Trains.ToList());

            // Act
            bool canCreateTrain = _viewmodel.NavigateToCreateTrainForStationViewCommand
                .CanExecute(null);

            // Assert
            Assert.False(canCreateTrain);
        }

        [Fact]
        public void DeleteAvailableTrain_CanDelete_RemovesAvailableTrain()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);
            _stationDALMock.Setup(d => d.GetTrainsInStation(_station.Id))
                .Returns(_station.TrainsInStation.ToList()); 
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                .Returns(new List<Train> { _trainNotInTestStation });

            _viewmodel.LoadTrainsForStation();
            _viewmodel.LoadAvailableTrains();

            _viewmodel.SelectedAvailableTrain = _trainNotInTestStation;

            _stationDALMock.Setup(d => d.DeleteTrainPermanently(_station.Id, _trainNotInTestStation.Id));
            _stationDALMock.Setup(d => d.GetTrainsForStation(_station.Id))
                        .Returns(new List<Train>());

            // Act
            _viewmodel.DeleteAvailableTrainCommand.Execute(null);

            // Assert
            _stationDALMock.Verify(d =>
                d.DeleteTrainPermanently(_station.Id, _trainNotInTestStation.Id),
                Times.Once
            );

            _viewmodel.LoadAvailableTrains();
            Assert.DoesNotContain(_trainNotInTestStation, _viewmodel.AvailableTrains);
        }

        [Fact]
        public void CanDeleteAvailableTrain_SelectedAvailableTrain_ReturnsTrue()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);

            _viewmodel.SelectedAvailableTrain = _trainNotInTestStation;

            // Act
            bool canDelete = _viewmodel.DeleteAvailableTrainCommand.CanExecute(null);

            // Assert
            Assert.True(canDelete);
        }

        [Fact]
        public void CanDeleteAvailableTrain_NoSelectedAvailableTrain_ReturnsFalse()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);
            _stationDALMock.Setup(d => d.FindById(_station.Id))
                .Returns(_station);

            // Act
            bool canDelete = _viewmodel.DeleteAvailableTrainCommand.CanExecute(null);

            // Assert
            Assert.False(canDelete);
        }

        [Fact]
        public void Close_Anytime_NavigatesToMap()
        {
            // Act
            _viewmodel.CloseCommand.Execute(null);

            // Assert
            _navigationServiceMock.Verify(
                n => n.NavigateTo<MapViewModel>(),
                Times.Once
            );
        }
    }
}

