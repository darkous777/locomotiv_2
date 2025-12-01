using System.Windows;
using System.Windows.Media;

namespace Locomotiv.Utils
{
    public static class MapConstants
    {
        public static readonly Point MainMarkerOffset = new Point(-16, -32);
        public static readonly Point InfoMarkerOffset = new Point(-100, -120);

        public const double InfoPanelWidth = 300;
        public const double ButtonWidth = 200;
        public const double CloseButtonWidth = 80;

        public const string BlockPointLabelPrefix = "🛤️";
        public const string TrainLabelPrefix = "🚆";

        public static readonly Brush StationColor = Brushes.Red;
        public static readonly Brush PointColor = Brushes.Green;
        public static readonly Brush BlockPointColor = Brushes.Black;
        public static readonly Brush TrainColor = Brushes.Blue;

        public const int TrainMovementIntervalSeconds = 2;
    }
}
