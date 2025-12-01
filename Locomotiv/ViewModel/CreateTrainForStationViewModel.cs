using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;
using System.Windows.Input;

namespace Locomotiv.ViewModel
{
    public class CreateTrainForStationViewModel : BaseViewModel
    {
        private readonly IStationDAL _stationDAL;
        private readonly IStationContextService _stationContextService;
        private readonly INavigationService _navigationService;
        private readonly ITrainDAL _trainDAL;
        private Station? _currentStation;

        private TrainType _selectedTrainType;
        private PriorityLevel _selectedPriorityLevel;
        private TrainState _selectedTrainState = TrainState.Programmed;
        private string _locomotiveCode;
        private int _numberOfWagons;
        private string _errorMessage;

        public List<TrainType> TrainTypes { get; }
        public List<PriorityLevel> PriorityLevels { get; }
        public List<TrainState> TrainStates { get; }

        public TrainType SelectedTrainType
        {
            get => _selectedTrainType;
            set
            {
                _selectedTrainType = value;
                OnPropertyChanged();
            }
        }

        public PriorityLevel SelectedPriorityLevel
        {
            get => _selectedPriorityLevel;
            set
            {
                _selectedPriorityLevel = value;
                OnPropertyChanged();
            }
        }

        public TrainState SelectedTrainState
        {
            get => _selectedTrainState;
            set
            {
                _selectedTrainState = value;
                OnPropertyChanged();
            }
        }

        public string LocomotiveCode
        {
            get => _locomotiveCode;
            set
            {
                _locomotiveCode = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfWagons
        {
            get => _numberOfWagons;
            set
            {
                if (value < 1)
                {
                    _numberOfWagons = 1;
                }
                else
                {
                    _numberOfWagons = value;
                }
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand CreateTrainCommand { get; }

        public ICommand CancelCommand { get; }

        public CreateTrainForStationViewModel(
            IStationDAL stationDAL,
            IStationContextService stationContextService,
            INavigationService navigationService,
            ITrainDAL trainDAL
        )
        {
            _stationDAL = stationDAL;
            _stationContextService = stationContextService;
            _navigationService = navigationService;
            _trainDAL = trainDAL;

            _currentStation = _stationContextService.CurrentStation;

            CreateTrainCommand = new RelayCommand(CreateTrain, CanCreateTrain);
            CancelCommand = new RelayCommand(Cancel);

            TrainTypes = Enum.GetValues(typeof(TrainType))
                .Cast<TrainType>().ToList();
            PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                .Cast<PriorityLevel>().ToList();
            TrainStates = Enum.GetValues(typeof(TrainState))
                .Cast<TrainState>().ToList();
        }

        private bool CanCreateTrain()
        {
            return !string.IsNullOrWhiteSpace(_locomotiveCode) && _numberOfWagons >= 1;
        }

        private void CreateTrain()
        {
            if (_currentStation == null)
            {
                ErrorMessage = "No station selected.";
                return;
            }

            try
            {
                Locomotive locomotive = new Locomotive
                {
                    Code = _locomotiveCode
                };

                List<Wagon> wagons = new List<Wagon>();
                for (int i = 0; i < _numberOfWagons; i++)
                {
                    wagons.Add(new Wagon
                    {
                        Code = $"WAG-{_locomotiveCode}-{i + 1}"
                    });
                }

                Train train = new Train
                {
                    TypeOfTrain = _selectedTrainType,
                    PriotityLevel = _selectedPriorityLevel,
                    State = _selectedTrainState,
                    Latitude = _currentStation.Latitude,
                    Longitude = _currentStation.Longitude,
                    Locomotives = new List<Locomotive> { locomotive },
                    Wagons = wagons
                };

                _stationDAL.CreateTrainForStation(_currentStation.Id, train);
                _navigationService.NavigateTo<TrainManagementViewModel>();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating train: {ex.Message}";
            }
        }

        private void Cancel()
        {
            _navigationService.NavigateTo<TrainManagementViewModel>();
        }
    }
}
