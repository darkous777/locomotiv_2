using GMap.NET;
using GMap.NET.WindowsPresentation;
using Locomotiv.Model;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.Utils.Services.Map;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Locomotiv.ViewModel
{
    public class MapViewModel : BaseViewModel
    {
        private readonly IStationDAL _stationDal;
        private readonly IBlockDAL _blockDal;
        private readonly IBlockPointDAL _blockPointDal;
        private readonly IPredefinedRouteDAL _predefinedRouteDal;
        private readonly ITrainDAL _trainDal;
        private readonly INavigationService _navigationService;
        private readonly IStationContextService _stationContextService;
        private readonly IUserSessionService _userSessionService;

        private readonly TrainMovementService _trainMovementService;
        private readonly MapMarkerFactory _markerFactory;
        private readonly MapInfoService _infoService;

        public ObservableCollection<GMapMarker> Markers { get; set; }

        private DispatcherTimer _movementTimer;
        private Dictionary<int, TrainMovementState> _activeTrains 
            = new Dictionary<int, TrainMovementState>();

        private Dictionary<int, (GMapMarker main, GMapMarker info)> _trainMarkers
            = new Dictionary<int, (GMapMarker, GMapMarker)>();

        private Dictionary<int, TextBlock> _stationInfoPanels = new Dictionary<int, TextBlock>();
        private Dictionary<int, TextBlock> _blockPointInfoPanels = new Dictionary<int, TextBlock>();
        private Dictionary<int, TextBlock> _trainInfoPanels = new Dictionary<int, TextBlock>();

        private HashSet<int> _usedRouteIds = new HashSet<int>();

        public ICommand StartAllTrainsCommand { get; }
        public ICommand StopAllTrainsCommand { get; }

        public MapViewModel(
            IStationDAL stationDal,
            IBlockDAL blockDal,
            IBlockPointDAL blockPointDal,
            IPredefinedRouteDAL predefinedRouteDal,
            ITrainDAL trainDal,
            INavigationService navigationService,
            IStationContextService stationContextService,
            IUserSessionService userSessionService,
            TrainMovementService trainMovementService,
            MapMarkerFactory markerFactory,
            MapInfoService infoService,
            bool loadPointsOnStartup = true)
        {
            _stationDal = stationDal;
            _blockDal = blockDal;
            _blockPointDal = blockPointDal;
            _predefinedRouteDal = predefinedRouteDal;
            _trainDal = trainDal;
            _navigationService = navigationService;
            _stationContextService = stationContextService;
            _userSessionService = userSessionService;
            _trainMovementService = trainMovementService;
            _markerFactory = markerFactory;
            _infoService = infoService;

            Markers = new ObservableCollection<GMapMarker>();

            _movementTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(MapConstants.TrainMovementIntervalSeconds)
            };
            _movementTimer.Tick += OnMovementTimerTick;

            StartAllTrainsCommand = new RelayCommand(StartAllTrainsWithRoutes);
            StopAllTrainsCommand = new RelayCommand(StopAllTrains);

            if (loadPointsOnStartup)
            {
                LoadPoints();
            }
        }

        private void LoadPoints()
        {
            LoadBlockPoints();

            if (_userSessionService.ConnectedUser?.IsAdmin == true)
            {
                LoadStations();
            }

            LoadTrainsOnBlocks();
        }

        private void LoadBlockPoints()
        {
            foreach (BlockPoint blockPoint in _blockPointDal.GetAll())
            {
                CreateMarkerForBlockPoint(blockPoint);
            }
        }

        private void LoadStations()
        {
            foreach (Station station in _stationDal.GetAll())
            {
                CreateMarkerForStation(station);
            }
        }

        private void LoadTrainsOnBlocks()
        {
            foreach (Block block in _blockDal.GetAll())
            {
                if (block.CurrentTrain != null)
                {
                    CreateMarkerForTrain(block.CurrentTrain, block);
                }
            }
        }

        private void CreateMarkerForStation(Station station)
        {
            Brush color = station.Type == StationType.Station
                ? MapConstants.StationColor
                : MapConstants.PointColor;

            Button manageButton = CreateManageTrainsButton(station);

            (GMapMarker mainMarker, GMapMarker infoMarker) 
                = _markerFactory.CreateMarkerPair(
                station, station.Name, color, _infoService.GetStationInfo(station),
                onInfoPanelCreated: panel => StoreInfoPanel(
                    station.Id, panel, _stationInfoPanels
                ),
                additionalButton: manageButton);

            Markers.Add(mainMarker);
            Markers.Add(infoMarker);
        }

        private void CreateMarkerForBlockPoint(BlockPoint blockPoint)
        {
            (GMapMarker mainMarker, GMapMarker infoMarker) 
                = _markerFactory.CreateMarkerPair(
                blockPoint,
                $"{MapConstants.BlockPointLabelPrefix}{blockPoint.Id}",
                MapConstants.BlockPointColor,
                _infoService.GetBlockPointInfo(blockPoint),
                onInfoPanelCreated: panel => StoreInfoPanel(
                    blockPoint.Id, panel, _blockPointInfoPanels
                ));

            Markers.Add(mainMarker);
            Markers.Add(infoMarker);
        }

        private void CreateMarkerForTrain(Train train, Block block)
        {
            (GMapMarker mainMarker, GMapMarker infoMarker) 
                = _markerFactory.CreateMarkerPair(
                block,
                $"{MapConstants.TrainLabelPrefix}{train.Id}",
                MapConstants.TrainColor,
                _infoService.GetTrainInfo(train),
                onInfoPanelCreated: panel => StoreInfoPanel(train.Id, panel, _trainInfoPanels));

            Markers.Add(mainMarker);
            Markers.Add(infoMarker);

            _trainMarkers[train.Id] = (mainMarker, infoMarker);
        }

        private Button CreateManageTrainsButton(Station station)
        {
            Button button = new Button
            {
                Content = "Ajouter/Supprimer un train",
                Width = MapConstants.ButtonWidth,
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                Tag = station
            };

            button.Click += (s, e) =>
            {
                if (s is Button btn && btn.Tag is Station st)
                {
                    OpenTrainManagementWindow(st);
                }
            };

            return button;
        }

        private void StoreInfoPanel(int id, Border panel, Dictionary<int, TextBlock> targetDictionary)
        {
            if (panel.Child is StackPanel stack && stack.Children.Count > 0)
            {
                if (stack.Children[0] is TextBlock textBlock)
                {
                    targetDictionary[id] = textBlock;
                }
            }
        }

        private void OpenTrainManagementWindow(Station station)
        {
            if (station == null) return;

            _stationContextService.CurrentStation = station;
            _navigationService.NavigateTo<TrainManagementViewModel>();
        }

        private void RefreshAllInfoPanels()
        {
            foreach (KeyValuePair<int, TextBlock> panelEntry in _stationInfoPanels)
            {
                Station? station = _stationDal.FindById(panelEntry.Key);
                if (station != null)
                {
                    panelEntry.Value.Text = _infoService.GetStationInfo(station);
                }
            }

            foreach (KeyValuePair<int, TextBlock> panelEntry in _blockPointInfoPanels)
            {
                BlockPoint? blockPoint = _blockPointDal.GetAll().FirstOrDefault(bp => bp.Id == panelEntry.Key);
                if (blockPoint != null)
                {
                    panelEntry.Value.Text = _infoService.GetBlockPointInfo(blockPoint);
                }
            }

            foreach (KeyValuePair<int, TextBlock> panelEntry in _trainInfoPanels)
            {
                if (_activeTrains.TryGetValue(panelEntry.Key, out TrainMovementState? state))
                {
                    panelEntry.Value.Text = _infoService.GetTrainInfo(state.Train);
                }
            }
        }

        private void OnMovementTimerTick(object sender, EventArgs e)
        {
            MoveTrains();
        }

        public void StartTrainMovement(Train train, PredefinedRoute route)
        {
            if (!ValidateTrainMovement(train, route))
                return;

            IList<Block> blocks = _stationDal.GetBlocksForPredefinedRoute(route.BlockIds);

            if (blocks == null || blocks.Count == 0)
                return;

            _trainMovementService.DepartFromAllStations(train.Id);

            TrainMovementState movementState = new TrainMovementState
            {
                Train = train,
                Route = route,
                Blocks = blocks,
                CurrentBlockIndex = 0,
                IsMoving = true
            };

            _activeTrains[train.Id] = movementState;

            _trainMovementService.PlaceTrainOnBlock(train, blocks[0]);
            UpdateOrCreateTrainMarker(train, blocks[0]);

            if (!_movementTimer.IsEnabled)
                _movementTimer.Start();
        }

        private bool ValidateTrainMovement(Train train, PredefinedRoute route)
        {
            return train != null &&
                   route != null &&
                   route.BlockIds != null &&
                   route.BlockIds.Count > 0;
        }

        public void StopAllTrains()
        {
            _activeTrains.Clear();
            _movementTimer.Stop();
        }

        private void MoveTrains()
        {
            List<int> trainsToRemove = new List<int>();

            foreach (KeyValuePair<int, TrainMovementState> activeTrain in _activeTrains)
            {
                if (!activeTrain.Value.IsMoving)
                    continue;

                bool trainCompleted = MoveTrainToNextBlock(activeTrain.Key, activeTrain.Value);

                if (trainCompleted)
                {
                    trainsToRemove.Add(activeTrain.Key);
                }
            }

            RemoveCompletedTrains(trainsToRemove);
            RefreshAllInfoPanels();

            if (_activeTrains.Count == 0)
                _movementTimer.Stop();
        }

        private bool MoveTrainToNextBlock(int trainId, TrainMovementState state)
        {
            Block currentBlock = state.Blocks[state.CurrentBlockIndex];
            state.CurrentBlockIndex++;

            if (state.CurrentBlockIndex >= state.Blocks.Count)
            {
                HandleTrainArrival(trainId, state, currentBlock);
                return true;
            }

            Block nextBlock = state.Blocks[state.CurrentBlockIndex];
            _trainMovementService.MoveTrainToBlock(state.Train, currentBlock, nextBlock);
            UpdateOrCreateTrainMarker(state.Train, nextBlock);

            return false;
        }

        private void HandleTrainArrival(int trainId, TrainMovementState state, Block currentBlock)
        {
            _trainMovementService.ClearTrainFromBlock(currentBlock);

            Station endStation = null;

            if (state.Route.EndStation != null)
            {
                endStation = _stationDal.FindById(state.Route.EndStation.Id);
                if (endStation != null)
                {
                    _trainMovementService.ArriveAtStation(state.Train, endStation);
                }
            }

            RemoveTrainMarker(trainId);

            AssignNewRouteFromEndStation(state.Train, endStation);
        }

        private void AssignNewRouteFromEndStation(Train train, Station endStation)
        {
            if (endStation == null)
            {
                train.PredefinedRoute = null;
                _trainDal.Update(train);
                return;
            }

            PredefinedRoute newRoute = FindRouteFromStation(endStation.Id);

            if (newRoute != null)
            {
                _usedRouteIds.Add(newRoute.Id);
            }

            train.PredefinedRoute = newRoute;
            _trainDal.Update(train);
        }

        private void RemoveCompletedTrains(List<int> trainIds)
        {
            foreach (int trainId in trainIds)
            {
                _activeTrains.Remove(trainId);
            }
        }

        private void UpdateOrCreateTrainMarker(Train train, Block block)
        {
            if (_trainMarkers.ContainsKey(train.Id))
            {
                UpdateTrainMarkerPosition(train.Id, block);
            }
            else
            {
                CreateMarkerForTrain(train, block);
            }
        }

        private void UpdateTrainMarkerPosition(int trainId, Block block)
        {
            PointLatLng newPosition = new PointLatLng(block.Latitude, block.Longitude);

            _trainMarkers[trainId].main.Position = newPosition;
            _trainMarkers[trainId].info.Position = newPosition;
        }

        private void RemoveTrainMarker(int trainId)
        {
            if (_trainMarkers.ContainsKey(trainId))
            {
                Markers.Remove(_trainMarkers[trainId].main);
                Markers.Remove(_trainMarkers[trainId].info);
                _trainMarkers.Remove(trainId);
            }

            _trainInfoPanels.Remove(trainId);
        }

        public void StartAllTrainsWithRoutes()
        {
            _usedRouteIds.Clear();

            IList<Train> allTrains = _stationDal.GetAllTrain();
            HashSet<int> trainIdsOnBlocks = GetTrainIdsCurrentlyOnBlocks();

            foreach (Train train in allTrains)
            {
                if (!CanStartTrain(train.Id, trainIdsOnBlocks))
                    continue;

                PredefinedRoute route = GetOrAssignRouteForTrain(train);

                if (route != null)
                {
                    _usedRouteIds.Add(route.Id);
                    StartTrainMovement(train, route);
                }
            }
        }

        private PredefinedRoute GetOrAssignRouteForTrain(Train train)
        {
            if (train.PredefinedRoute != null)
            {
                if (_usedRouteIds.Contains(train.PredefinedRoute.Id))
                {
                    return null;
                }
                return train.PredefinedRoute;
            }

            Station currentStation = FindTrainCurrentStation(train);
            if (currentStation == null)
                return null;

            PredefinedRoute newRoute = FindRouteFromStation(currentStation.Id);
            if (newRoute != null)
            {
                AssignRouteToTrain(train, newRoute);
            }

            return newRoute;
        }

        private Station FindTrainCurrentStation(Train train)
        {
            IList<Station> stations = _stationDal.GetAll();

            foreach (Station station in stations)
            {
                if (station.TrainsInStation?.Any(t => t.Id == train.Id) == true)
                    return station;

                if (station.Trains?.Any(t => t.Id == train.Id) == true)
                    return station;
            }

            return null;
        }

        private PredefinedRoute FindRouteFromStation(int stationId)
        {
            IList<PredefinedRoute> routes = _predefinedRouteDal.GetAll();
            return routes.FirstOrDefault(r =>
                r.StartStation?.Id == stationId &&
                !_usedRouteIds.Contains(r.Id));
        }

        private void AssignRouteToTrain(Train train, PredefinedRoute route)
        {
            train.PredefinedRoute = route;
            _trainDal.Update(train);
        }

        private HashSet<int> GetTrainIdsCurrentlyOnBlocks()
        {
            return _blockDal.GetTrainsCurrentlyOnBlocks()
                .Select(t => t.Id)
                .ToHashSet();
        }

        private bool CanStartTrain(int trainId, HashSet<int> trainIdsOnBlocks)
        {
            return !_activeTrains.ContainsKey(trainId) &&
                   !trainIdsOnBlocks.Contains(trainId);
        }

        internal string GetStationInfo(Station station)
        {
            return _infoService.GetStationInfo(station);
        }

        internal string GetBlockInfo(BlockPoint blockPoint)
        {
            return _infoService.GetBlockPointInfo(blockPoint);
        }

    }
}
