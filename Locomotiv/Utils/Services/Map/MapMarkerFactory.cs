using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;

namespace Locomotiv.Utils.Services.Map
{
    public class MapMarkerFactory
    {
        public (GMapMarker mainMarker, GMapMarker infoMarker) CreateMarkerPair(
            IMapPoint mapPoint,
            string label,
            Brush color,
            string infoText,
            Action<Border> onInfoPanelCreated = null,
            Button additionalButton = null)
        {
            PointLatLng position = new PointLatLng(mapPoint.Latitude, mapPoint.Longitude);

            GMapMarker mainMarker = CreateMainMarker(position, label, color);
            GMapMarker infoMarker = CreateInfoMarker(position, infoText, additionalButton);

            Button button = mainMarker.Shape as Button;
            Border infoPanel = infoMarker.Shape as Border;

            button.Click += (s, e) => ToggleInfoPanelVisibility(infoPanel);

            onInfoPanelCreated?.Invoke(infoPanel);

            return (mainMarker, infoMarker);
        }

        private GMapMarker CreateMainMarker(PointLatLng position, string label, Brush color)
        {
            Button button = new Button
            {
                Content = label,
                Background = color,
                Foreground = Brushes.White,
                Padding = new Thickness(8, 2, 8, 2),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            return new GMapMarker(position)
            {
                Offset = MapConstants.MainMarkerOffset,
                Shape = button
            };
        }

        private GMapMarker CreateInfoMarker(PointLatLng position, string infoText, Button additionalButton = null)
        {
            Border panel = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(8),
                Width = MapConstants.InfoPanelWidth,
                Visibility = Visibility.Hidden
            };

            StackPanel stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = infoText,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            });

            if (additionalButton != null)
            {
                stack.Children.Add(additionalButton);
            }

            stack.Children.Add(CreateCloseButton(panel));

            panel.Child = stack;

            return new GMapMarker(position)
            {
                Offset = MapConstants.InfoMarkerOffset,
                Shape = panel
            };
        }

        private Button CreateCloseButton(Border panel)
        {
            Button closeBtn = new Button
            {
                Content = "Fermer",
                Width = MapConstants.CloseButtonWidth,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            closeBtn.Click += (s, e) => panel.Visibility = Visibility.Hidden;

            return closeBtn;
        }

        private void ToggleInfoPanelVisibility(Border infoPanel)
        {
            infoPanel.Visibility = infoPanel.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;
        }
    }
}
