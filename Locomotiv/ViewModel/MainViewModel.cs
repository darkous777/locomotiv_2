using System.Windows.Input;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;

namespace Locomotiv.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;

        public INavigationService NavigationService
        {
            get => _navigationService;
        }

        public IUserSessionService UserSessionService
        {
            get => _userSessionService;
        }

        public ICommand NavigateToConnectUserViewCommand { get; set; }

        public ICommand NavigateToHomeViewCommand { get; set; }

        public ICommand NavigateToMapViewCommand { get; set; }

        public ICommand NavigateToStationDetailsViewCommand { get; set; }

        public ICommand DisconnectCommand { get; }

        public ICommand NavigateToReserveTicketViewCommand { get; set; }

        public MainViewModel(
            INavigationService navigationService, 
            IUserSessionService userSessionService
        )
        {
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            
            NavigateToConnectUserViewCommand = new RelayCommand(() 
                => NavigationService.NavigateTo<ConnectUserViewModel>());
            NavigateToHomeViewCommand = new RelayCommand(() 
                => NavigationService.NavigateTo<HomeViewModel>());
            NavigateToMapViewCommand = new RelayCommand(() 
                => NavigationService.NavigateTo<MapViewModel>());

            DisconnectCommand = new RelayCommand(
                Disconnect, () => UserSessionService.IsUserConnected
            );

            NavigateToReserveTicketViewCommand = new RelayCommand(()
                => NavigationService.NavigateTo<ReserveTicketViewModel>());

            NavigationService.NavigateTo<HomeViewModel>();
        }

        private void Disconnect()
        {
            _userSessionService.ConnectedUser = null;
            OnPropertyChanged(nameof(UserSessionService.IsUserConnected));
            _navigationService.NavigateTo<ConnectUserViewModel>();
        }
    }
}
