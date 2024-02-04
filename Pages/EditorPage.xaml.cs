using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using MeasurePixels.Measure;
using MeasurePixels.ViewModels;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace MeasurePixels.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditorPage : Page
    {
        public bool CanPaste
        {
            get { return (bool)GetValue(CanPasteProperty); }
            set { SetValue(CanPasteProperty, value); }
        }
        public static readonly DependencyProperty CanPasteProperty =
            DependencyProperty.Register(
                nameof(CanPaste),
                typeof(bool),
                typeof(EditorPage),
                new PropertyMetadata(false));
        public bool DetailVisiable
        {
            get { return (bool)GetValue(DetailVisiableProperty); }
            set { SetValue(DetailVisiableProperty, value); }
        }
        public static readonly DependencyProperty DetailVisiableProperty =
            DependencyProperty.Register(
                nameof(DetailVisiable),
                typeof(bool),
                typeof(EditorPage),
                new PropertyMetadata(false));

        public EditorPage()
        {
            this.InitializeComponent();
            SetupCapture();

            SystemNavigationManagerPreview.GetForCurrentView()
                 .CloseRequested += OnWindowCloseRequested;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Editor page Loaded");
            Clipboard.ContentChanged += Clipboard_ContentChanged;
            Window.Current.Activated += Current_Activated;
        }

        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != Windows.UI.Core.CoreWindowActivationState.Deactivated)
                Clipboard_ContentChanged(sender, e);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Editor page unloaded");
            Clipboard.ContentChanged -= Clipboard_ContentChanged;
            Window.Current.Activated -= Current_Activated;
            this.StopCapture();
        }

        private async void Clipboard_ContentChanged(object sender, object e)
        {
            CanPaste = await this.measureCanvas.CheckClipboard();
            //Debug.WriteLine("Clipboard_ContentChanged:" + CanPaste);
        }

        private async void Clipboard_Click(object sender, RoutedEventArgs e)
        {
            await this.measureCanvas.Paste();
        }

        private void ShotButton_Click(object sender, RoutedEventArgs e)
        {
            StartMonitorCapture();
        }

        private async void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            await StartCaptureAsync();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => this.measureCanvas.Clear();
        private void UndoButton_Click(object sender, RoutedEventArgs e) => this.measureCanvas.Undo();
        private void RedoButton_Click(object sender, RoutedEventArgs e) => this.measureCanvas.Redo();
        private void DeleteButton_Click(object sender, RoutedEventArgs e) => this.measureCanvas.Delete();

        private void SidePane_Click(object sender, RoutedEventArgs e)
        {
            var resources = ResourceLoader.GetForCurrentView();
            DetailVisiable = !DetailVisiable;
            var button = sender as AppBarToggleButton;
            (button.Icon as SymbolIcon).Symbol = DetailVisiable ?
                 Symbol.OpenPane : Symbol.DockRight;
            button.SetValue(ToolTipService.ToolTipProperty,
                resources.GetString(
                    DetailVisiable ? "CloseSideBar" : "OpenSideBar"));
        }

        private void measureCanvas_OnSelectedChanged(object sender, Measure.Objects.MeasureObject e)
        {
            this.measureDetail.SetObject(e);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await PickFile();
            if (file == null) return;
            var tag = (sender as FrameworkElement).Tag?.ToString();
            await this.measureCanvas.SaveAsync(file, tag == "Measures");
        }

        private async Task<StorageFile> PickFile()
        {
            var resources = ResourceLoader.GetForCurrentView();

            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                DefaultFileExtension = ".png",
                SuggestedFileName = resources.GetString("AppDisplayName") + " - " +
                                    DateTime.Now.ToString("yyyyMMddHHmmss")
            };
            picker.FileTypeChoices.Add("PNG", new[] { ".png" });
            picker.FileTypeChoices.Add("JPG", new[] { ".jpg", ".jpeg", ".jpe" });
            picker.FileTypeChoices.Add("BMP", new[] { ".bmp" });
            //picker.FileTypeChoices.Add("GIF", new[] { ".gif" });
            picker.FileTypeChoices.Add("TIFF", new[] { ".tif",".tiff" });
            return await picker.PickSaveFileAsync();
        }

        private async void OnWindowCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            Debug.WriteLine("EditorPage.OnWindowCloseRequested");

            if (AppSettings.Current.AskSaveOnClose && measureCanvas.CanMeasure)
            {
                e.Handled = true;
                var re = await new Controls.AskSaveDialog().ShowAsync();
                if (re == ContentDialogResult.Primary)
                {
                    var file = await PickFile();
                    if (file == null) return;
                    await this.measureCanvas.SaveAsync(file, false);
                    Environment.Exit(0);
                }
                else if (re == ContentDialogResult.Secondary)
                    Environment.Exit(0);
            }
        }

        private void MeasureToolBarKeyInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var key = args.KeyboardAccelerator.Key;
            switch (key)
            {
                case VirtualKey.Escape:
                case VirtualKey.Number1:
                    measureCanvas.ActiveTool = MeasureTool.Pointer;
                    break;
                case VirtualKey.Number2:
                    measureCanvas.ActiveTool = MeasureTool.Segment;
                    break;
                case VirtualKey.Number3:
                    measureCanvas.ActiveTool = MeasureTool.Angle;
                    break;
                case VirtualKey.Number4:
                    measureCanvas.ActiveTool = MeasureTool.Edge;
                    break;
                case VirtualKey.Number5:
                    measureCanvas.ActiveTool = MeasureTool.Eyedropper;
                    break;
            }
        }
    }
}
