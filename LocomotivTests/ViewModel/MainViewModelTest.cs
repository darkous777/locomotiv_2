using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;

namespace LocomotivTests.ViewModel
{
    public class MainViewModelTest
    {
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IUserSessionService> _userSessionServiceMock;
        private readonly MainViewModel _viewmodel;

        public MainViewModelTest()
        {
            _navigationServiceMock = new Mock<INavigationService>();
            _userSessionServiceMock = new Mock<IUserSessionService>();

            _viewmodel = new MainViewModel(
                _navigationServiceMock.Object,
                _userSessionServiceMock.Object
            );
        }

        [Fact]
        public void Disconnect_ConnectedUser_LogsOutUser()
        {
            // Arrange
            _userSessionServiceMock.SetupGet(c => c.IsUserConnected)
                .Returns(true);

            // Act
            _viewmodel.DisconnectCommand.Execute(null);

            // Assert: should log out user
            _userSessionServiceMock.VerifySet(c => c.ConnectedUser = null,
                Times.Once);
        }

        [Fact]
        public void Disconnect_ConnectedUser_NavigatesToConnectUser()
        {
            // Arrange
            _userSessionServiceMock.SetupGet(c => c.IsUserConnected)
                .Returns(true);

            // Act
            _viewmodel.DisconnectCommand.Execute(null);

            // Assert: should navigate to connect user
            _navigationServiceMock.Verify(n => n.NavigateTo<ConnectUserViewModel>(),
                Times.Once);
        }
    }
}