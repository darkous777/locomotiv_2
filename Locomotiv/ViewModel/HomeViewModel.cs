using Locomotiv.Model;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Locomotiv.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly IUserDAL _userDAL;
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;
        private readonly IStationDAL _stationDAL;
        private readonly IPredefinedRouteDAL _predefinedRouteDAL;
        private readonly ITrainDAL _trainDAL;
        private readonly ITicketDAL _ticketDAL; // Add ticket DAL

        public User? ConnectedUser
        {
            get => _userSessionService.ConnectedUser;
        }

        public string WelcomeMessage
        {
            get => ConnectedUser == null ?
                "Bienvenue chère personne inconnue!" : $"Bienvenue {ConnectedUser.Prenom}!";
        }

        public bool IsAdmin => ConnectedUser?.IsAdmin ?? false;

        public bool IsEmployee => ConnectedUser != null && !ConnectedUser.IsAdmin && ConnectedUser.Type != EmployeeType.None;

        public bool IsClient => ConnectedUser != null && !ConnectedUser.IsAdmin && ConnectedUser.Type == EmployeeType.None;

        private Station? _employeeStation;
        private string _stationName;
        private int _stationCapacity;
        private int _trainsAssignedCount;
        private int _trainsInStationCount;
        private ICollection<Train>? _trainsAssigned;
        private ICollection<Train>? _trainsInStation;

        public Station? EmployeeStation
        {
            get => ConnectedUser?.Station;
            set => _employeeStation = value;
        }

        public string StationName
        {
            get => EmployeeStation?.Name ?? "Non assigné";
            set => _stationName = value;
        }

        public int StationCapacity
        {
            get => EmployeeStation?.Capacity ?? 0;
            set => _stationCapacity = value;
        }

        public int TrainsAssignedCount
        {
            get => EmployeeStation?.Trains?.Count ?? 0;
            set => _trainsAssignedCount = value;
        }

        public int TrainsInStationCount
        {
            get => EmployeeStation?.TrainsInStation?.Count ?? 0;
            set => _trainsInStationCount = value;
        }

        public ICollection<Train>? TrainsAssigned
        {
            get => EmployeeStation?.Trains;
            set => _trainsAssigned = value;
        }

        public ICollection<Train>? TrainsInStation
        {
            get => EmployeeStation?.TrainsInStation;
            set => _trainsInStation = value;
        }

        // Client Tickets Properties
        private ObservableCollection<Ticket> _clientTickets;
        public ObservableCollection<Ticket> ClientTickets
        {
            get => _clientTickets;
            set
            {
                _clientTickets = value;
                OnPropertyChanged(nameof(ClientTickets));
            }
        }

        private bool _hasTickets;
        public bool HasTickets
        {
            get => _hasTickets;
            set
            {
                _hasTickets = value;
                OnPropertyChanged(nameof(HasTickets));
            }
        }

        private int? _totalStations;
        private int? _totalTrains;
        private int? _totalTrainsInStations;
        private int? _totalAvailableTrains;
        private int? _totalWagons;
        private int? _totalLocomotives;

        public int TotalStations
        {
            get
            {
                if (!_totalStations.HasValue && IsAdmin)
                {
                    _totalStations = _stationDAL?.GetAll()?.Count ?? 0;
                }
                return _totalStations ?? 0;
            }
            set { _totalStations = value; }
        }

        public int TotalTrains
        {
            get
            {
                if (!_totalTrains.HasValue && IsAdmin)
                {
                    _totalTrains = _stationDAL?.GetAllTrain()?.Count() ?? 0;
                }
                return _totalTrains ?? 0;
            }
            set { _totalTrains = value; }
        }

        public int TotalTrainsInStations
        {
            get
            {
                int compteur = 0;
                if (!_totalTrainsInStations.HasValue && IsAdmin)
                {
                    IList<Station> stations = _stationDAL?.GetAll();
                    foreach (Station station in stations ?? new List<Station>())
                    {
                        if (station.TrainsInStation is not null)
                        {
                            compteur += station.TrainsInStation.Count();
                        }
                    }
                }
                return compteur;
            }
            set { _totalTrainsInStations = value; }
        }

        public int TotalAvailableTrains
        {
            get
            {
                int compteur = 0;
                if (!_totalAvailableTrains.HasValue && IsAdmin)
                {
                    IList<Station> stations = _stationDAL?.GetAll();
                    foreach (Station station in stations ?? new List<Station>())
                    {
                        if (station.Trains is not null)
                        {
                            compteur += station.Trains.Count();
                        }
                    }
                }
                return compteur;
            }
            set { _totalAvailableTrains = value; }
        }

        public int TotalWagons
        {
            get
            {
                int compteur = 0;
                if (!_totalWagons.HasValue && IsAdmin)
                {
                    IList<Station> stations = _stationDAL?.GetAll();
                    foreach (Station station in stations ?? new List<Station>())
                    {
                        compteur += station.Trains?
                            .Sum(t => t.Wagons?.Count() ?? 0) ?? 0;
                        compteur += station.TrainsInStation?
                            .Sum(t => t.Wagons?.Count() ?? 0) ?? 0;
                    }
                }
                return compteur;
            }
            set { _totalWagons = value; }
        }

        public int TotalLocomotives
        {
            get
            {
                int compteur = 0;

                if (!_totalLocomotives.HasValue && IsAdmin)
                {
                    IList<Station> stations = _stationDAL?.GetAll();
                    foreach (Station station in stations ?? new List<Station>())
                    {
                        compteur += station.Trains?.Sum(t => t.Locomotives?.Count() ?? 0) ?? 0;
                        compteur += station.TrainsInStation?
                            .Sum(t => t.Locomotives?.Count() ?? 0) ?? 0;
                    }
                }
                return compteur;
            }
            set { _totalLocomotives = value; }
        }

        private Station _selectedStartStation;
        private Station _selectedEndStation;
        private Train _selectedTrain;
        public IList<Station> AllStations { get; }
        public ObservableCollection<Train> TrainsForSelectedStation { get; private set; }
        public ObservableCollection<Station> AvailableEndStations { get; private set; }
        public Train SelectedTrain
        {
            get => _selectedTrain;
            set
            {
                _selectedTrain = value;
                OnPropertyChanged(nameof(SelectedTrain));
            }
        }

        public Station SelectedStartStation
        {
            get => _selectedStartStation;
            set
            {
                _selectedStartStation = value;
                OnPropertyChanged(nameof(SelectedStartStation));

                UpdateAvailableTrains();
                UpdateAvailableEndStations();
            }
        }

        public Station SelectedEndStation
        {
            get => _selectedEndStation;
            set
            {
                _selectedEndStation = value;
                OnPropertyChanged(nameof(SelectedEndStation));
            }
        }

        private string _selectedRouteSummary;
        public string SelectedRouteSummary
        {
            get {
                return _selectedRouteSummary;
            }
            set
            {
                _selectedRouteSummary = value;
                OnPropertyChanged(nameof(SelectedRouteSummary));
            }
        }

        public HomeViewModel(
            IUserDAL userDAL,
            INavigationService navigationService,
            IUserSessionService userSessionService,
            IStationDAL stationDAL,
            IPredefinedRouteDAL predefinedRouteDAL,
            ITrainDAL trainDAL,
            ITicketDAL ticketDAL // Add ticket DAL parameter
        )
        {
            _userDAL = userDAL;
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            _stationDAL = stationDAL;
            _predefinedRouteDAL = predefinedRouteDAL;
            _trainDAL = trainDAL;
            _ticketDAL = ticketDAL;

            AllStations = _stationDAL.GetAll();
            TrainsForSelectedStation = new ObservableCollection<Train>();
            AvailableEndStations = new ObservableCollection<Station>(AllStations);
            ClientTickets = new ObservableCollection<Ticket>();

            LogoutCommand = new RelayCommand(Logout, CanLogout);
            FindRouteCommand = new RelayCommand(FindRoute, CanFindRoute);

            // Load client tickets if user is a client
            if (IsClient)
            {
                LoadClientTickets();
            }
        }

        public ICommand LogoutCommand { get; set; }

        private void Logout()
        {
            _userSessionService.ConnectedUser = null;
            _navigationService.NavigateTo<ConnectUserViewModel>();
        }

        private bool CanLogout()
        {
            return _userSessionService.IsUserConnected;
        }

        private void LoadClientTickets()
        {
            if (ConnectedUser != null)
            {
                var tickets = _ticketDAL.GetTicketsByUser(ConnectedUser.Id);
                ClientTickets.Clear();

                foreach (var ticket in tickets ?? new List<Ticket>())
                {
                    ClientTickets.Add(ticket);
                }

                HasTickets = ClientTickets.Count > 0;
            }
        }

        private void UpdateAvailableTrains()
        {
            TrainsForSelectedStation.Clear();

            if (SelectedStartStation != null)
            {
                IList<Train> trains = _stationDAL.GetTrainsInStation(SelectedStartStation.Id);

                foreach (Train train in trains)
                    TrainsForSelectedStation.Add(train);
            }

            OnPropertyChanged(nameof(TrainsForSelectedStation));
        }

        public ICommand FindRouteCommand { get; }

        private bool CanFindRoute()
        {
            return SelectedStartStation != null
                   && SelectedEndStation != null
                   && SelectedTrain != null;
        }

        private void FindRoute()
        {
            IList<PredefinedRoute> routes = _predefinedRouteDAL.GetAll();

            PredefinedRoute? route = routes.FirstOrDefault(r =>
                r.StartStation.Id == SelectedStartStation.Id &&
                r.EndStation.Id == SelectedEndStation.Id
            );

            if (route == null)
            {
                SelectedRouteSummary = "Aucun itinéraire trouvé entre ces deux stations.";
                return;
            }

            SelectedRouteSummary =
                $"Itinéraire trouvé : {route.Name}\n" +
                $"Nombre de blocs : {route.BlockIds.Count}";

            AddRouteToTrain(route);
        }

        private void AddRouteToTrain(PredefinedRoute predefinedRoute)
        {
            SelectedTrain.PredefinedRoute = predefinedRoute;
            _trainDAL.Update(SelectedTrain);
        }

        private void UpdateAvailableEndStations()
        {
            AvailableEndStations.Clear();

            if (SelectedStartStation == null)
            {
                foreach (Station station in AllStations)
                    AvailableEndStations.Add(station);

                return;
            }

            List<PredefinedRoute> routes = _predefinedRouteDAL.GetAll()
                .Where(r => r.StartStation.Id == SelectedStartStation.Id)
                .ToList();

            foreach (Station station in routes.Select(r => r.EndStation).Distinct())
                AvailableEndStations.Add(station);
        }
    }
}