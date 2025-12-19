using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;

namespace LocomotivTests.ViewModel
{
    public class ReserveTicketViewModelTest
    {
        private readonly Mock<ITrainDAL> _trainDALMock;
        private readonly ReserveTicketViewModel _viewmodel;
        private readonly Train _trainWithSeats;
        private readonly Train _trainWithoutSeats;
        private readonly Mock<ITicketDAL> _ticketDAL;
        private readonly Mock<IStationDAL> _stationDAL;
        private readonly Mock<IUserSessionService> _userSessionService;
        private readonly Mock<ILoggingService> _loggingService;

        public ReserveTicketViewModelTest()
        {
            _trainDALMock = new Mock<ITrainDAL>();
            _ticketDAL = new Mock<ITicketDAL>();
            _stationDAL = new Mock<IStationDAL>();
            _userSessionService = new Mock<IUserSessionService>();
            _loggingService = new Mock<ILoggingService>();

            _trainWithSeats = new Train
            {
                Id = 1,
                TypeOfTrain = TrainType.Passenger,
                Locomotives = new List<Locomotive>
                {
                    new Locomotive { Id = 1, Code = "Loco-001", PassengerCapacity = 10 }
                }
            };

            _trainWithoutSeats = new Train
            {
                Id = 2,
                TypeOfTrain = TrainType.Passenger,
                Locomotives = new List<Locomotive>()
            };

            _trainDALMock.Setup(d => d.GetAllAvailablePassengerTrains())
                .Returns(new List<Train>());

            _viewmodel = new ReserveTicketViewModel(_trainDALMock.Object, _ticketDAL.Object, _stationDAL.Object, _userSessionService.Object, _loggingService.Object);
        }

        [Fact]
        public void GetTrainWithAvailableSeats_TrainsWithSeats_ReturnsOnlyTrainsWithSeats()
        {
            // Arrange
            _trainDALMock.Setup(d => d.GetAllAvailablePassengerTrains())
                .Returns(new List<Train>
                {
                    _trainWithSeats,
                    _trainWithoutSeats,
                });

            // Act
            List<Train> result = _viewmodel.GetTrainWithAvailableSeats();

            // Assert
            Assert.Contains(_trainWithSeats, result);
            Assert.DoesNotContain(_trainWithoutSeats, result);
        }

        [Fact]
        public void GetTrainWithAvailableSeats_NoTrainsWithSeats_ReturnsNothing()
        {
            // Arrange
            _trainDALMock.Setup(d => d.GetAllAvailablePassengerTrains())
                .Returns(new List<Train>
                {
                    _trainWithoutSeats
                });

            // Act
            List<Train> result = _viewmodel.GetTrainWithAvailableSeats();

            // Assert
            Assert.Empty(result);
        }
    }
}

