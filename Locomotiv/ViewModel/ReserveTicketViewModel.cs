using Locomotiv.Model;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Locomotiv.ViewModel
{
    public class ReserveTicketViewModel : BaseViewModel
    {
        private readonly ITrainDAL _trainDAL;
        private readonly ITicketDAL _ticketDAL;
        private readonly IStationDAL _stationDAL;
        private readonly IUserSessionService _userSessionService;
        private readonly ILoggingService _loggingService;

        private List<Train> _allAvailableTrains;

        public ObservableCollection<Train> FilteredTrains { get; set; }
        public ObservableCollection<Station> AllStations { get; set; }

        public bool HasSelectedTrain => SelectedTrain != null;

        private Train? _selectedTrain;
        public Train? SelectedTrain
        {
            get => _selectedTrain;
            set
            {
                _selectedTrain = value;
                OnPropertyChanged(nameof(SelectedTrain));
                OnPropertyChanged(nameof(HasSelectedTrain));
            }
        }

        private DateTime? _selectedDate = null;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                ApplyFilters();
            }
        }

        private Station? _selectedDestination;
        public Station? SelectedDestination
        {
            get => _selectedDestination;
            set
            {
                _selectedDestination = value;
                OnPropertyChanged(nameof(SelectedDestination));
                ApplyFilters();
            }
        }

        private string _filterDepartureTime = string.Empty;
        public string FilterDepartureTime
        {
            get => _filterDepartureTime;
            set
            {
                _filterDepartureTime = value;
                OnPropertyChanged(nameof(FilterDepartureTime));
                ApplyFilters();
            }
        }

        private string _confirmationMessage = string.Empty;
        public string ConfirmationMessage
        {
            get => _confirmationMessage;
            set
            {
                _confirmationMessage = value;
                OnPropertyChanged(nameof(ConfirmationMessage));
            }
        }

        public ICommand ReserveTicketCommand { get; set; }
        public ICommand ClearFiltersCommand { get; set; }

        public ReserveTicketViewModel(
            ITrainDAL trainDAL,
            ITicketDAL ticketDAL,
            IStationDAL stationDAL,
            IUserSessionService userSessionService,
            ILoggingService loggingService)
        {
            _trainDAL = trainDAL;
            _ticketDAL = ticketDAL;
            _stationDAL = stationDAL;
            _userSessionService = userSessionService;
            _loggingService = loggingService;

            AllStations = new ObservableCollection<Station>(_stationDAL.GetAll());
            FilteredTrains = new ObservableCollection<Train>();

            ReserveTicketCommand = new RelayCommand(ReserveTicket, CanReserveTicket);
            ClearFiltersCommand = new RelayCommand(ClearFilters, () => true);

            LoadAvailableTrains();
        }

        private void LoadAvailableTrains()
        {
            _allAvailableTrains = _trainDAL.GetAllAvailablePassengerTrains()
                .Where(t => t.AvailableSeats > 0 && t.PredefinedRoute != null)
                .OrderBy(t => t.PredefinedRoute!.DepartureTime)
                .ToList();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allAvailableTrains.AsEnumerable();

            if (SelectedDestination != null)
            {
                filtered = filtered.Where(t =>
                    t.PredefinedRoute!.EndStation.Id == SelectedDestination.Id);
            }

            if (!string.IsNullOrWhiteSpace(FilterDepartureTime))
            {
                if (TimeSpan.TryParse(FilterDepartureTime, out TimeSpan filterTime))
                {
                    filtered = filtered.Where(t =>
                        t.PredefinedRoute!.DepartureTime.TimeOfDay >= filterTime);
                }
            }

            if (SelectedDate.HasValue)
            {
                filtered = filtered.Where(t =>
                    t.PredefinedRoute!.DepartureTime.Date == SelectedDate.Value.Date);
            }

            FilteredTrains.Clear();
            foreach (var train in filtered.OrderBy(t => t.PredefinedRoute!.DepartureTime))
            {
                FilteredTrains.Add(train);
            }
        }

        private void ClearFilters()
        {
            SelectedDestination = null;
            FilterDepartureTime = string.Empty;
            SelectedDate = null;
            ConfirmationMessage = string.Empty;
        }

        private bool CanReserveTicket()
        {
            return SelectedTrain != null &&
                   SelectedTrain.AvailableSeats > 0 &&
                   _userSessionService.ConnectedUser != null;
        }

        private void ReserveTicket()
        {
            if (SelectedTrain == null || _userSessionService.ConnectedUser == null)
                return;

            try
            {
                var ticket = new Ticket
                {
                    Train = SelectedTrain,
                    User = _userSessionService.ConnectedUser
                };

                _ticketDAL.Add(ticket);

                _loggingService.LogInfo($"Billet réservé - Train #{SelectedTrain.Id}, Utilisateur: {_userSessionService.ConnectedUser.Username}");

                ConfirmationMessage = $"Billet réservé avec succès pour le train #{SelectedTrain.Id} - {SelectedTrain.PredefinedRoute?.Name}";

                LoadAvailableTrains();
                SelectedTrain = null;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Erreur lors de la réservation du billet", ex);
                ConfirmationMessage = "Erreur lors de la réservation. Veuillez réessayer.";
            }
        }
    }
}
