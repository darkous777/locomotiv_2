using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;

namespace LocomotivTests.ViewModel
{
    public class ConnectUserViewModelTest
    {
        private readonly Mock<IUserDAL> _userDALMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IUserSessionService> _userSessionServiceMock;
        private readonly Mock<ILoggingService> _loggingServiceMock;
        private readonly ConnectUserViewModel _viewmodel;
        private readonly User _user;

        public ConnectUserViewModelTest()
        {
            _userDALMock = new Mock<IUserDAL>();
            _navigationServiceMock = new Mock<INavigationService>();
            _userSessionServiceMock = new Mock<IUserSessionService>();
            _loggingServiceMock = new Mock<ILoggingService>();

            _viewmodel = new ConnectUserViewModel(
                _userDALMock.Object,
                _navigationServiceMock.Object,
                _userSessionServiceMock.Object,
                _loggingServiceMock.Object
            );

            _user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = "password123"
            };
        }

        [Fact]
        public void Connect_ValidUsernameAndPassword_ConnectsUser()
        {
            // Arrange
            _viewmodel.Username = "testuser";
            _viewmodel.Password = "password123";

            _userDALMock.Setup(
                d => d.FindByUsernameAndPassword(
                _viewmodel.Username, _viewmodel.Password)
            ).Returns(_user);

            // Act
            _viewmodel.ConnectCommand.Execute(null);

            // Assert: should connect user
            _userSessionServiceMock.VerifySet(c => c.ConnectedUser = _user,
               Times.Once);
        }

        [Fact]
        public void Connect_InValidUsernameOrPassword_DoesNotConnectUser()
        {
            // Arrange
            _viewmodel.Username = "testuser";
            _viewmodel.Password = "password1234";

            _userDALMock.Setup(
                d => d.FindByUsernameAndPassword(
                _viewmodel.Username, _viewmodel.Password)
            ).Returns((User?)null);

            // Act
            _viewmodel.ConnectCommand.Execute(null);

            // Assert: should not connect user
            _userSessionServiceMock.VerifySet(c => c.ConnectedUser = _user, 
                Times.Never);
        }

        [Fact]
        public void Connect_ValidUsernameAndPassword_NavigatesToHome()
        {
            // Arrange
            _viewmodel.Username = "testuser";
            _viewmodel.Password = "password123";

            _userDALMock.Setup(
                f => f.FindByUsernameAndPassword(
                _viewmodel.Username, _viewmodel.Password)
            ).Returns(_user);

            // Act
            _viewmodel.ConnectCommand.Execute(null);

            // Assert: should navigate to home
            _navigationServiceMock.Verify(n => n.NavigateTo<HomeViewModel>(), 
                Times.Once);
        }

        [Fact]
        public void Connect_InValidUsernameOrPassword_DoesNotNavigateToHome()
        {
            // Arrange
            _viewmodel.Username = "testuser";
            _viewmodel.Password = "password1234";

            _userDALMock.Setup(
                d => d.FindByUsernameAndPassword(
                _viewmodel.Username, _viewmodel.Password)
            ).Returns((User?)null);

            // Act
            _viewmodel.ConnectCommand.Execute(null);

            // Assert: should not navigate to home
            _navigationServiceMock.Verify(n => n.NavigateTo<HomeViewModel>(), 
                Times.Never);
        }

        [Fact]
        public void Connect_InValidUsernameOrPassword_AddsError()
        {
            // Arrange
            _viewmodel.Username = "testuser";
            _viewmodel.Password = "password1234";

            _userDALMock.Setup(
                d => d.FindByUsernameAndPassword(
                _viewmodel.Username, _viewmodel.Password)
            ).Returns((User?)null);

            // Act
            _viewmodel.ConnectCommand.Execute(null);

            // Assert: should have errors
            Assert.True(_viewmodel.HasErrors);
        }

        [Fact]
        public void CanConnect_NotEmptyUsernameAndPassword_ReturnsTrue()
        {
            // Arrange
            _viewmodel.Username = "blabla";
            _viewmodel.Password = "blabla";

            // Act
            bool canConnect = _viewmodel.ConnectCommand.CanExecute(null);

            // Assert: should be able to connect
            Assert.True(canConnect);
        }

        [Fact]
        public void CanConnect_EmptyUsernameOrPassword_ReturnsFalse()
        {
            // Arrange
            _viewmodel.Username = "testuser";
            _viewmodel.Password = "";

            // Act
            bool canConnect = _viewmodel.ConnectCommand.CanExecute(null);

            // Asser: should not be able to connect
            Assert.False(canConnect);
        }

        [Fact]
        public void ValidateProperty_ValidUsernameAndPassword_DoesNotAddErrors()
        {
            // Arrange
            _viewmodel.Username = "testuser";
            _viewmodel.Password = "password123";

            // Act
            bool hasErrors = _viewmodel.HasErrors;

            // Assert: should not have errors
            Assert.False(hasErrors);
        }

        [Fact]
        public void ValidateProperty_EmptyUsername_AddsError()
        {
            // Arrange
            _viewmodel.Username = "";
            _viewmodel.Password = "password123";

            // Act
            bool hasErrors = _viewmodel.HasErrors;

            // Assert: should have errors
            Assert.True(hasErrors);
        }

        [Fact]
        public void ValidateProperty_TooShortUsername_AddsError()
        {
            // Arrange
            _viewmodel.Username = "t";
            _viewmodel.Password = "password123";

            // Act
            bool hasErrors = _viewmodel.HasErrors;

            // Assert: should have errors
            Assert.True(hasErrors);
        }

        [Fact]
        public void ValidateProperty_EmptyPassword_AddsError()
        {
            // Arrange
            _viewmodel.Username = "testuser";
            _viewmodel.Password = "";

            // Act
            bool hasErrors = _viewmodel.HasErrors;

            // Assert: should have errors
            Assert.True(hasErrors);
        }
    }
}
