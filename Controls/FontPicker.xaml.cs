using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.Graphics.Canvas.Text;

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

namespace MeasurePixels.Controls
{

    public sealed partial class FontPicker : UserControl
    {
        public static List<string> FontsNames;

        static FontPicker()
        {
            FontsNames = new List<string>(CanvasTextFormat.GetSystemFontFamilies());
            FontsNames.Sort();
        }

        public event EventHandler VauleChanged;
        public List<string> FontsList => FontsNames;

        public FontPicker()
        {
            this.InitializeComponent();
        }

        public int TextFontSize
        {
            get { return (int)GetValue(TextFontSizeProperty); }
            set { SetValue(TextFontSizeProperty, value); }
        }

        public static readonly DependencyProperty TextFontSizeProperty =
            DependencyProperty.Register(
                nameof(TextFontSize),
                typeof(int),
                typeof(FontPicker),
                new PropertyMetadata(14, PropertyChanged));

        public string TextColor
        {
            get { return (string)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register(
                nameof(TextColor),
                typeof(string),
                typeof(FontPicker),
                new PropertyMetadata("#000", PropertyChanged));

        public string TextBackground
        {
            get { return (string)GetValue(TextBackgroundProperty); }
            set { SetValue(TextBackgroundProperty, value); }
        }

        public static readonly DependencyProperty TextBackgroundProperty =
            DependencyProperty.Register(
                nameof(TextBackground),
                typeof(string),
                typeof(FontPicker),
                new PropertyMetadata("#FFF", PropertyChanged));

        public string TextFontFamily
        {
            get { return (string)GetValue(TextFontFamilyProperty); }
            set { SetValue(TextFontFamilyProperty, value); }
        }

        public static readonly DependencyProperty TextFontFamilyProperty =
            DependencyProperty.Register(
                nameof(TextFontFamily),
                typeof(string),
                typeof(FontPicker),
                new PropertyMetadata(FontFamily.XamlAutoFontFamily.Source, PropertyChanged));

        public int MaximumFontSize
        {
            get { return (int)GetValue(MaximumFontSizeProperty); }
            set { SetValue(MaximumFontSizeProperty, value); }
        }

        public static readonly DependencyProperty MaximumFontSizeProperty =
            DependencyProperty.Register(
                nameof(TextFontSize),
                typeof(int),
                typeof(FontPicker),
                new PropertyMetadata(96));

        public int MinimumFontSize
        {
            get { return (int)GetValue(MinimumFontSizeProperty); }
            set { SetValue(MinimumFontSizeProperty, value); }
        }

        public static readonly DependencyProperty MinimumFontSizeProperty =
            DependencyProperty.Register(
                nameof(TextFontSize),
                typeof(int),
                typeof(FontPicker),
                new PropertyMetadata(8));

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = d as FontPicker;
            picker?.VauleChanged?.Invoke(picker, EventArgs.Empty);
        }

        FlyoutShowOptions showOptions = new FlyoutShowOptions
        {
            ShowMode = FlyoutShowMode.Auto,
            Placement = FlyoutPlacementMode.TopEdgeAlignedLeft
        };
        private void FontColor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            element.ContextFlyout.ShowAt(element, showOptions);
        }
    }
}
