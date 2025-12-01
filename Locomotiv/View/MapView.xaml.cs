using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Locomotiv.Model;
using Locomotiv.ViewModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Locomotiv.View
{
    public partial class MapView : UserControl
    {
        private MapViewModel _vm;

        public MapView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = DataContext as MapViewModel;

            MapControl.MapProvider = OpenStreetMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            MapControl.Position = new PointLatLng(46.795817, -71.309985);
            MapControl.Zoom = 12;
            MapControl.DragButton = MouseButton.Left;
            MapControl.CanDragMap = true;
            MapControl.ShowCenter = false;

            foreach (GMapMarker marker in _vm.Markers)
                MapControl.Markers.Add(marker);

            _vm.Markers.CollectionChanged += Markers_CollectionChanged;
        }

        private void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (GMapMarker marker in e.NewItems)
                    MapControl.Markers.Add(marker);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (GMapMarker marker in e.OldItems)
                    MapControl.Markers.Remove(marker);
            }
        }
    }
}
