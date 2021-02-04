using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.UI.Xaml.Media.Animation;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Core.Preview;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using System;
using Windows.ApplicationModel.Resources;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MeasurePixels
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current { get; private set; }

        AppSettings settings;
        public MainPage()
        {
            Current = this;
            this.InitializeComponent();
            settings = AppSettings.Current;

            //添加关闭窗口监听
            //需要在Package.appxmanifest中配置rescap
            SystemNavigationManagerPreview.GetForCurrentView()
                    .CloseRequested += MainPage_CloseRequested;

            this.UseCustomTitleBar();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.contentFrame.Navigate(typeof(Pages.EditorPage));
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            this.contentFrame.GoBack();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (this.contentFrame.SourcePageType != typeof(Pages.SettingsPage))
            {
                this.contentFrame.Navigate(typeof(Pages.SettingsPage), "Settings",
                    new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
            }
            else
            {
                if (view.TryEnterFullScreenMode())
                    ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            }
        }

        private async void contentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            var resources = ResourceLoader.GetForCurrentView();

            ContentDialog dialog = new ContentDialog
            {
                CloseButtonText = resources.GetString("Ok"),
                Title = resources.GetString("NavigtionFailedTitle"),
                Content = resources.GetString("NavigtionFailedContent")
            };
            Debug.WriteLine("Failed to load Page " + e.SourcePageType.FullName + ". "
                + Environment.NewLine + e.Exception.ToString());
            await dialog.ShowAsync();
        }

        private void MainPage_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            Debug.WriteLine("MainPage_CloseRequested");
        }

        #region TitleBar
        private void UseCustomTitleBar()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = GetForegroundColorByTheme();
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);
            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);
            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            //active state handle
            //Window.Current.Activated += Current_Activated;
        }

        public Color GetForegroundColorByTheme(FrameworkElement sender = null)
        {
            sender = sender ?? this;
            Color color = Colors.Black;
            if (sender.RequestedTheme == ElementTheme.Dark
                || (sender.RequestedTheme == ElementTheme.Default
                && Application.Current.RequestedTheme == ApplicationTheme.Dark))
                color = Colors.White;
            return color;
        }

        private void Page_ActualThemeChanged(FrameworkElement sender, object args)
        {
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor =
                GetForegroundColorByTheme(sender);
        }

        //private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        //{
        //    switch (e.WindowActivationState)
        //    {
        //        case Windows.UI.Core.CoreWindowActivationState.Deactivated:
        //            AppTitleBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
        //            break;
        //        case Windows.UI.Core.CoreWindowActivationState.PointerActivated:
        //        case Windows.UI.Core.CoreWindowActivationState.CodeActivated:
        //        default:
        //            AppTitleBar.Background = (Brush)App.Current.Resources["appbarAcrylicBrush"];
        //            break;
        //    }
        //}

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);

            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);

            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = Math.Max(coreTitleBar.Height + 3, AppTitleBar.Height);
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }


        #endregion

    }
}
