using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Locomotiv.ViewModel
{
    public class TrainManagementViewModel : BaseViewModel
    {
        private readonly IStationDAL _stationDAL;
        private readonly IStationContextService _stationContextService;
        private readonly INavigationService _navigationService;
        private Station? _currentStation;

        public ObservableCollection<Train> Trains { get; set; }

        public ObservableCollection<Train> AvailableTrains { get; set; }

        private Train? _selectedTrain;
        public Train? SelectedTrain
        {
            get => _selectedTrain;
            set
            {
                _selectedTrain = value;
                OnPropertyChanged();
            }
        }

        private Train? _selectedAvailableTrain;
        public Train? SelectedAvailableTrain
        {
            get => _selectedAvailableTrain;
            set
            {
                _selectedAvailableTrain = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CapacityText
        {
            get
            {
                if (_currentStation == null)
                {
                    return "0/0";
                }
                return $"{Trains.Count}/{_currentStation.Capacity}";
            }
        }

        public ICommand AddTrainCommand { get; set; }

        public ICommand DeleteTrainCommand { get; set; }

        public ICommand DeleteAvailableTrainCommand { get; set; }

        public ICommand NavigateToCreateTrainForStationViewCommand { get; set; }

        public ICommand CloseCommand { get; set; }

        public TrainManagementViewModel(
            IStationDAL stationDAL,
            IStationContextService stationContextService,
            INavigationService navigationService
        )
        {
            _stationDAL = stationDAL;
            _stationContextService = stationContextService;
            _navigationService = navigationService;

            Trains = new ObservableCollection<Train>();
            AvailableTrains = new ObservableCollection<Train>();

            AddTrainCommand = new RelayCommand(
                AddTrain, CanAddTrain
            );
            NavigateToCreateTrainForStationViewCommand = new RelayCommand(
                CreateTrain, CanCreateTrain
            );
            DeleteTrainCommand = new RelayCommand(
                DeleteTrain, CanDeleteTrain
            );
            DeleteAvailableTrainCommand = new RelayCommand(
                DeleteAvailableTrain, CanDeleteAvailableTrain
            );
            CloseCommand = new RelayCommand(Close);

            LoadData();
        }


        private void LoadData()
        {
            LoadTrainsForStation();
            LoadAvailableTrains();

            OnPropertyChanged(nameof(CapacityText));

            CommandManager.InvalidateRequerySuggested();
        }

        internal void LoadTrainsForStation()
        {
            Trains.Clear();

            _currentStation = _stationContextService.CurrentStation;

            if (_currentStation != null)
            {
                _currentStation = _stationDAL.FindById(_currentStation.Id);

                if (_currentStation != null)
                {
                    IList<Train> trainsInStation = _stationDAL.GetTrainsInStation(_currentStation.Id);

                    foreach (Train train in trainsInStation)
                    {
                        Trains.Add(train);
                    }
                }
            }
        }

        internal void LoadAvailableTrains()
        {
            AvailableTrains.Clear();

            if (_currentStation != null)
            {
                // Handle potential null return from DAL
                IList<Train> availableTrains = _stationDAL.GetTrainsForStation(_currentStation.Id) ?? new List<Train>();

                foreach (Train train in availableTrains)
                {
                    AvailableTrains.Add(train);
                }
            }
        }

        internal void CreateTrain()
        {
            _stationContextService.CurrentStation = _currentStation;

            _navigationService.NavigateTo<CreateTrainForStationViewModel>();
        }

        internal bool CanCreateTrain()
        {
            return _currentStation != null;
        }

        internal void AddTrain()
        {
            if (SelectedAvailableTrain != null && _currentStation != null)
            {
                _currentStation = _stationDAL.FindById(_currentStation.Id);

                if (_currentStation != null)
                {
                    _stationDAL.AddTrainToStation(_currentStation.Id, SelectedAvailableTrain.Id, addToTrainsInStation: true);

                    LoadData();
                    SelectedAvailableTrain = null;
                }
            }
        }

        internal void DeleteTrain()
        {
            _currentStation = _stationDAL.FindById(_currentStation.Id);

            if (SelectedTrain != null && _currentStation != null)
            {
                _stationDAL.RemoveTrainFromStation(_currentStation.Id, SelectedTrain.Id);

                LoadData();
                SelectedTrain = null;
            }
        }

        internal bool CanAddTrain()
        {
            if (SelectedAvailableTrain == null || _currentStation == null)
            {
                return false;
            }

            return Trains.Count < _currentStation.Capacity;
        }

        internal bool CanDeleteTrain()
        {
            return SelectedTrain != null;
        }

        internal void DeleteAvailableTrain()
        {
            if (SelectedAvailableTrain != null && _currentStation != null)
            {
                _currentStation = _stationDAL.FindById(_currentStation.Id);

                if (_currentStation != null)
                {
                    _stationDAL.DeleteTrainPermanently(_currentStation.Id, SelectedAvailableTrain.Id);

                    LoadData();
                    SelectedAvailableTrain = null;
                }
            }
        }

        internal bool CanDeleteAvailableTrain()
        {
            return SelectedAvailableTrain != null;
        }

        internal void Close()
        {
            _navigationService.NavigateTo<MapViewModel>();
        }
    }
}
