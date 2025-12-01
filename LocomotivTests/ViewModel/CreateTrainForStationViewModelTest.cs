using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;

namespace LocomotivTests.ViewModel
{
    public class CreateTrainForStationViewModelTest
    {
        private readonly Mock<IStationDAL> _stationDALMock;
        private readonly Mock<IStationContextService> _stationContextServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<ITrainDAL> _trainDALMock;
        private readonly CreateTrainForStationViewModel _viewmodel;
        private readonly Station _station;

        public CreateTrainForStationViewModelTest()
        {
            _stationDALMock = new Mock<IStationDAL>();
            _stationContextServiceMock = new Mock<IStationContextService>();
            _navigationServiceMock = new Mock<INavigationService>();
            _trainDALMock = new Mock<ITrainDAL>();

            _viewmodel = new CreateTrainForStationViewModel(
                _stationDALMock.Object,
                _stationContextServiceMock.Object,
                _navigationServiceMock.Object,
                _trainDALMock.Object
            );

            _station = new Station
            {
                Id = 1,
                Name = "Test Station",
                Longitude = -71.204255,
                Latitude = 46.842256,
                Type = StationType.Station,
            };
        }

        [Fact]
        public void CanCreateTrain_ValidData_ReturnsTrue()
        {
            // Arrange
            _viewmodel.LocomotiveCode = "ABC123";
            _viewmodel.NumberOfWagons = 3;

            // Act
            bool canCreate = _viewmodel.CreateTrainCommand
                .CanExecute(null);

            // Assert
            Assert.True(canCreate);
        }

        [Fact]
        public void CanCreateTrain_InvalidCode_ReturnsFalse()
        {
            // Arrange
            _viewmodel.LocomotiveCode = "";
            _viewmodel.NumberOfWagons = 3;

            // Act
            bool canCreate = _viewmodel.CreateTrainCommand
                .CanExecute(null);

            // Assert
            Assert.False(canCreate);
        }

        [Fact]
        public void CanCreateTrain_NoWagons_AddsOneWagon_ReturnsTrue()
        {
            // Arrange
            _viewmodel.LocomotiveCode = "ABC123";
            _viewmodel.NumberOfWagons = 0;

            // Act
            bool canCreate = _viewmodel.CreateTrainCommand
                .CanExecute(null);
            int expected = 1;

            // Assert
            Assert.Equal(_viewmodel.NumberOfWagons, expected);
            Assert.True(canCreate);
        }

        [Fact]
        public void CreateTrain_ValidData_CreatesTrain()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns(_station);

            CreateTrainForStationViewModel viewmodel 
                = new CreateTrainForStationViewModel(
                _stationDALMock.Object,
                _stationContextServiceMock.Object,
                _navigationServiceMock.Object,
                _trainDALMock.Object
            );

            viewmodel.LocomotiveCode = "ABC123";
            viewmodel.NumberOfWagons = 3;
            viewmodel.SelectedTrainType = TrainType.Passenger;
            viewmodel.SelectedPriorityLevel = PriorityLevel.High;
            viewmodel.SelectedTrainState = TrainState.Programmed;

            // Act
            viewmodel.CreateTrainCommand.Execute(null);

            // Assert
            _stationDALMock.Verify(
                d => d.CreateTrainForStation(
                    _station.Id,
                    It.Is<Train>(t =>
                        t.TypeOfTrain == TrainType.Passenger &&
                        t.PriotityLevel == PriorityLevel.High &&
                        t.State == TrainState.Programmed &&
                        t.Latitude == _station.Latitude &&
                        t.Longitude == _station.Longitude &&
                        t.Locomotives.Count == 1 &&
                        t.Wagons.Count == 3
                    )
                ),
                Times.Once
            );

            _navigationServiceMock.Verify(n =>
                n.NavigateTo<TrainManagementViewModel>(),
                Times.Once);

            Assert.True(string.IsNullOrEmpty(_viewmodel.ErrorMessage));
        }
        
        [Fact]
        public void CreateTrain_NoSelectedStation_ReturnsErrorMessage()
        {
            // Arrange
            _stationContextServiceMock.SetupGet(s => s.CurrentStation)
                .Returns((Station?)null);

            CreateTrainForStationViewModel viewmodel 
                = new CreateTrainForStationViewModel(
                _stationDALMock.Object,
                _stationContextServiceMock.Object,
                _navigationServiceMock.Object,
                _trainDALMock.Object
            );

            viewmodel.LocomotiveCode = "ABC123";
            viewmodel.NumberOfWagons = 3;
            viewmodel.SelectedTrainType = TrainType.Passenger;
            viewmodel.SelectedPriorityLevel = PriorityLevel.High;
            viewmodel.SelectedTrainState = TrainState.Programmed;

            // Act
            viewmodel.CreateTrainCommand.Execute(null);

            // Assert
            _stationDALMock.Verify(
                d => d.CreateTrainForStation(
                    _station.Id,
                    It.Is<Train>(t =>
                        t.TypeOfTrain == TrainType.Passenger &&
                        t.PriotityLevel == PriorityLevel.High &&
                        t.State == TrainState.Programmed &&
                        t.Latitude == _station.Latitude &&
                        t.Longitude == _station.Longitude &&
                        t.Locomotives.Count == 1 &&
                        t.Wagons.Count == 3
                    )
                ),
                Times.Never
            );

            _navigationServiceMock.Verify(n =>
                n.NavigateTo<TrainManagementViewModel>(),
                Times.Never);

            Assert.False(string.IsNullOrEmpty(viewmodel.ErrorMessage));
        }

        [Fact]
        public void Cancel_Anytime_NavigatesToTrainManagement()
        {
            // Act
            _viewmodel.CancelCommand.Execute(null);

            // Assert
            _navigationServiceMock.Verify(
                n => n.NavigateTo<TrainManagementViewModel>(),
                Times.Once
            );
        }
    }
}
