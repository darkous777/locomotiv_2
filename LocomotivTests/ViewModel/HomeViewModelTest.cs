using System.Collections.ObjectModel;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;

namespace LocomotivTests.ViewModel
{
    public class HomeViewModelTest
    {
        private readonly Mock<IUserDAL> _userDALMock;
        private readonly Mock<IStationDAL> _stationDALMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IUserSessionService> _userSessionServiceMock;
        private readonly Mock<IPredefinedRouteDAL> _predefinedRouteDALMock;
        private readonly Mock<ITrainDAL> _trainDALMock;
        private readonly HomeViewModel _viewmodel;

        public HomeViewModelTest()
        {
            _userDALMock = new Mock<IUserDAL>();
            _stationDALMock = new Mock<IStationDAL>();
            _navigationServiceMock = new Mock<INavigationService>();
            _userSessionServiceMock = new Mock<IUserSessionService>();
            _predefinedRouteDALMock = new Mock<IPredefinedRouteDAL>();
            _trainDALMock = new Mock<ITrainDAL>();

            _stationDALMock.Setup(dal => dal.GetAll())
                .Returns(new List<Station>());

            _viewmodel = new HomeViewModel(
                _userDALMock.Object,
                _navigationServiceMock.Object,
                _userSessionServiceMock.Object,
                _stationDALMock.Object,
                _predefinedRouteDALMock.Object,
                _trainDALMock.Object
            );
        }

        [Fact]
        public void Logout_ConnectedUser_LogsOutUser()
        {
            // Arrange
            _userSessionServiceMock.SetupGet(c => c.IsUserConnected)
                .Returns(true);

            // Act
            _viewmodel.LogoutCommand.Execute(null);

            // Assert: should log out user
            _userSessionServiceMock.VerifySet(c => c.ConnectedUser = null,
                Times.Once);
        }

        [Fact]
        public void Logout_ConnectedUser_NavigatesToConnectUser()
        {
            // Arrange
            _userSessionServiceMock.SetupGet(c => c.IsUserConnected)
                .Returns(true);

            // Act
            _viewmodel.LogoutCommand.Execute(null);

            // Assert: should navigate to connect user
            _navigationServiceMock.Verify(n => n.NavigateTo<ConnectUserViewModel>(),
                Times.Once);
        }

        [Fact]
        public void CanLogout_ConnectedUser_ReturnsTrue()
        {
            // Arrange
            _userSessionServiceMock.SetupGet(c => c.IsUserConnected)
                .Returns(true);

            // Act
            bool canLogout = _viewmodel.LogoutCommand.CanExecute(null);

            // Assert: should be able to log out
            Assert.True(canLogout);
        }

        [Fact]
        public void CanLogout_NoConnectedUser_ReturnsFalse()
        {
            // Arrange
            _userSessionServiceMock.SetupGet(c => c.IsUserConnected)
                .Returns(false);

            // Act
            bool canLogout = _viewmodel.LogoutCommand.CanExecute(null);

            // Assert: should not be able to log out
            Assert.False(canLogout);
        }
    }
}