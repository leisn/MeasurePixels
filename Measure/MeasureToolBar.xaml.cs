using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using MeasurePixels.ViewModels;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MeasurePixels.Measure
{
    public enum MeasureTool
    {
        Pointer = 0,
        Segment = 1,
        Angle = 2,
        Edge = 3,
        Eyedropper
    }

    public sealed partial class MeasureToolBar : UserControl
    {
        public MeasureCanvas TargetCanvas
        {
            get => (MeasureCanvas)GetValue(TargetCanvasProperty);
            set => SetValue(TargetCanvasProperty, value);
        }

        public static readonly DependencyProperty TargetCanvasProperty =
          DependencyProperty.Register(
              nameof(TargetCanvas),
              typeof(MeasureCanvas),
              typeof(MeasureToolBar),
              new PropertyMetadata(null));

        FlyoutShowOptions showOptions;
        Flyout SharedFlyout;

        public MeasureToolBar()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SharedFlyout = this.Resources["SharedFlyout"] as Flyout;
            (SharedFlyout.Content as MeasurePenConf).OnPenChanged += OnPenChanged;
            showOptions = new FlyoutShowOptions
            {
                ShowMode = FlyoutShowMode.Auto,
                Placement = FlyoutPlacementMode.Auto
            };
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            showOptions = null;
            if (SharedFlyout != null)
            {
                (SharedFlyout.Content as MeasurePenConf).OnPenChanged -= OnPenChanged;
                SharedFlyout = null;
            }
        }

        private void OnPenChanged(MeasureTool tool, Objects.MeasurePen pen)
        {
            if (TargetCanvas != null)
                TargetCanvas.Pens[tool] = pen;
        }

        private void ItemRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var button = sender as RadioButton;
            if (button != null)
            {
                if (button.IsChecked != true)
                    button.IsChecked = true;
                ShowFlyout(button);
            }
        }
        private void ShowFlyout(RadioButton button)
        {
            var tag = button.Tag;
            var tool = (MeasureTool)int.Parse(tag.ToString());
            var content = SharedFlyout.Content as MeasurePenConf;
            content.SetPen(tool, TargetCanvas.Pens[tool]);
            SharedFlyout.ShowAt(button, showOptions);
        }

    }
}
