using Locomotiv.Model;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;

namespace LocomotivTests.ViewModel
{
    public class ReserveTicketViewModelTest
    {
        private readonly Mock<ITrainDAL> _trainDALMock;
        private readonly Mock<ITicketDAL> _ticketDALMock;
        private readonly Mock<IStationDAL> _stationDALMock;
        private readonly Mock<IUserSessionService> _userSessionServiceMock;
        private readonly Mock<ILoggingService> _loggingServiceMock;

        private readonly Train _trainWithSeatsAndRoute;
        private readonly Train _trainWithSeatsNoRoute;
        private readonly Train _trainWithoutSeatsWithRoute;
        private readonly Train _trainWithSeatsAndDifferentRoute;

        public ReserveTicketViewModelTest()
        {
            _trainDALMock = new Mock<ITrainDAL>();
            _ticketDALMock = new Mock<ITicketDAL>();
            _stationDALMock = new Mock<IStationDAL>();
            _userSessionServiceMock = new Mock<IUserSessionService>();
            _loggingServiceMock = new Mock<ILoggingService>();

            _stationDALMock.Setup(s => s.GetAll()).Returns(new List<Station>());

            Station endStation = new Station { Id = 2, Name = "End Station" };
            Station anotherStation = new Station { Id = 3, Name = "Another Station" };

            _trainWithSeatsAndRoute = new Train
            {
                Id = 1,
                TypeOfTrain = TrainType.Passenger,
                Locomotives = new List<Locomotive> { new Locomotive { PassengerCapacity = 10 } },
                Tickets = new List<Ticket>(),
                PredefinedRoute = new PredefinedRoute
                {
                    Id = 1,
                    Name = "Route 1",
                    EndStation = endStation
                }
            };

            _trainWithSeatsNoRoute = new Train
            {
                Id = 2,
                TypeOfTrain = TrainType.Passenger,
                Locomotives = new List<Locomotive> { new Locomotive { PassengerCapacity = 10 } },
                Tickets = new List<Ticket>()
            };

            _trainWithoutSeatsWithRoute = new Train
            {
                Id = 3,
                TypeOfTrain = TrainType.Passenger,
                Locomotives = new List<Locomotive>(),
                Tickets = new List<Ticket>(),
                PredefinedRoute = new PredefinedRoute
                {
                    Id = 2,
                    Name = "Route 2",
                    EndStation = endStation
                }
            };

            _trainWithSeatsAndDifferentRoute = new Train
            {
                Id = 4,
                TypeOfTrain = TrainType.Passenger,
                Locomotives = new List<Locomotive> { new Locomotive { PassengerCapacity = 10 } },
                Tickets = new List<Ticket>(),
                PredefinedRoute = new PredefinedRoute
                {
                    Id = 3,
                    Name = "Route 3",
                    EndStation = anotherStation
                }
            };
        }

        [Fact]
        public void LoadAvailableTrains_ShouldLoad_OnlyTrainsWithSeatsAndRoute()
        {
            // Arrange
            List<Train> trains = new List<Train> { _trainWithSeatsAndRoute, _trainWithSeatsNoRoute, _trainWithoutSeatsWithRoute };
            _trainDALMock.Setup(d => d.GetAllAvailablePassengerTrains()).Returns(trains);

            // Act
            ReserveTicketViewModel viewModel = new ReserveTicketViewModel(
                _trainDALMock.Object,
                _ticketDALMock.Object,
                _stationDALMock.Object,
                _userSessionServiceMock.Object,
                _loggingServiceMock.Object);

            // Assert
            Assert.Single(viewModel.FilteredTrains);
            Assert.Contains(_trainWithSeatsAndRoute, viewModel.FilteredTrains);
            Assert.DoesNotContain(_trainWithSeatsNoRoute, viewModel.FilteredTrains);
            Assert.DoesNotContain(_trainWithoutSeatsWithRoute, viewModel.FilteredTrains);
        }

        [Fact]
        public void LoadAvailableTrains_WhenNoValidTrains_FilteredTrainsIsEmpty()
        {
            // Arrange
            List<Train> trains = new List<Train> { _trainWithSeatsNoRoute, _trainWithoutSeatsWithRoute };
            _trainDALMock.Setup(d => d.GetAllAvailablePassengerTrains()).Returns(trains);

            // Act
            ReserveTicketViewModel viewModel = new ReserveTicketViewModel(
                _trainDALMock.Object,
                _ticketDALMock.Object,
                _stationDALMock.Object,
                _userSessionServiceMock.Object,
                _loggingServiceMock.Object);

            // Assert
            Assert.Empty(viewModel.FilteredTrains);
        }

        [Fact]
        public void ApplyFilters_WhenDestinationIsSelected_FiltersTrains()
        {
            // Arrange
            List<Train> trains = new List<Train> { _trainWithSeatsAndRoute, _trainWithSeatsAndDifferentRoute };
            _trainDALMock.Setup(d => d.GetAllAvailablePassengerTrains()).Returns(trains);
            ReserveTicketViewModel viewModel = new ReserveTicketViewModel(
                _trainDALMock.Object,
                _ticketDALMock.Object,
                _stationDALMock.Object,
                _userSessionServiceMock.Object,
                _loggingServiceMock.Object);

            // Act
            viewModel.SelectedDestination = _trainWithSeatsAndDifferentRoute.PredefinedRoute.EndStation;

            // Assert
            Assert.Single(viewModel.FilteredTrains);
            Assert.Equal(_trainWithSeatsAndDifferentRoute, viewModel.FilteredTrains.First());
        }

        [Fact]
        public void CanReserveTicket_WhenIsGoodToReserve_ReturnsTrue()
        {
            // Arrange
            _trainDALMock.Setup(d => d.GetAllAvailablePassengerTrains()).Returns(new List<Train>());
            ReserveTicketViewModel viewModel = new ReserveTicketViewModel(
                _trainDALMock.Object,
                _ticketDALMock.Object,
                _stationDALMock.Object,
                _userSessionServiceMock.Object,
                _loggingServiceMock.Object);

            viewModel.SelectedTrain = _trainWithSeatsAndRoute;
            _userSessionServiceMock.Setup(s => s.ConnectedUser).Returns(new User());

            // Act
            bool canReserve = viewModel.ReserveTicketCommand.CanExecute(null);

            // Assert
            Assert.True(canReserve);
        }

        [Fact]
        public void CanReserveTicket_WhenNoTrainSelected_ReturnsFalse()
        {
            // Arrange
            _trainDALMock.Setup(d => d.GetAllAvailablePassengerTrains()).Returns(new List<Train>());
            ReserveTicketViewModel viewModel = new ReserveTicketViewModel(
                _trainDALMock.Object,
                _ticketDALMock.Object,
                _stationDALMock.Object,
                _userSessionServiceMock.Object,
                _loggingServiceMock.Object);

            viewModel.SelectedTrain = null;
            _userSessionServiceMock.Setup(s => s.ConnectedUser).Returns(new User());

            // Act
            bool canReserve = viewModel.ReserveTicketCommand.CanExecute(null);

            // Assert
            Assert.False(canReserve);
        }

        [Fact]
        public void CanReserveTicket_WhenTrainHasNoSeats_ReturnsFalse()
        {
            // Arrange
            _trainDALMock.Setup(d => d.GetAllAvailablePassengerTrains()).Returns(new List<Train>());
            ReserveTicketViewModel viewModel = new ReserveTicketViewModel(
                _trainDALMock.Object,
                _ticketDALMock.Object,
                _stationDALMock.Object,
                _userSessionServiceMock.Object,
                _loggingServiceMock.Object);

            viewModel.SelectedTrain = _trainWithoutSeatsWithRoute;
            _userSessionServiceMock.Setup(s => s.ConnectedUser).Returns(new User());

            // Act
            bool canReserve = viewModel.ReserveTicketCommand.CanExecute(null);

            // Assert
            Assert.False(canReserve);
        }

        [Fact]
        public void CanReserveTicket_WhenNoUserConnected_ReturnsFalse()
        {
            // Arrange
            _trainDALMock.Setup(d => d.GetAllAvailablePassengerTrains()).Returns(new List<Train>());
            ReserveTicketViewModel viewModel = new ReserveTicketViewModel(
                _trainDALMock.Object,
                _ticketDALMock.Object,
                _stationDALMock.Object,
                _userSessionServiceMock.Object,
                _loggingServiceMock.Object);

            viewModel.SelectedTrain = _trainWithSeatsAndRoute;
            _userSessionServiceMock.Setup(s => s.ConnectedUser).Returns((User)null);

            // Act
            bool canReserve = viewModel.ReserveTicketCommand.CanExecute(null);

            // Assert
            Assert.False(canReserve);
        }
    }
}