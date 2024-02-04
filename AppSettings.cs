using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeasurePixels.ViewModels;

using Windows.Storage;
using Windows.UI.Xaml;

namespace MeasurePixels
{
    public class AppSettings : BaseViewModel
    {
        private static AppSettings current;
        public static AppSettings Current => current = current ?? new AppSettings();

        const string Settings_Theme = "app_theme";
        const string Settings_AskSaveOnClose = "save_on_close";
        const string Settings_Magnifier_Radius = "magnifier_radius";
        const string Settings_Magnifier_Scale = "magnifier_scale";
        const string Settings_Magnifier_Crosshairs = "magnifier_Crosshairs";
        static AppSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings.Values;
            if (localSettings.TryGetValue(Settings_AskSaveOnClose, out var save))
                Current._AskSaveOnClose = (bool)save;
            if (localSettings.TryGetValue(Settings_Theme, out var theme))
                Current._Theme = (ElementTheme)theme;
            if (localSettings.TryGetValue(Settings_Magnifier_Radius, out var mSize))
                Current._MagnifierRadius = (int)mSize;
            if (localSettings.TryGetValue(Settings_Magnifier_Scale, out var mScale))
                Current._MagnifierScale = (int)mScale;
            if (localSettings.TryGetValue(Settings_Magnifier_Crosshairs, out var mCross))
                Current._MagnifierShowCrosshairs = (bool)mCross;
        }

        public void SaveSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[Settings_Theme] = (int)Theme;
            localSettings.Values[Settings_AskSaveOnClose] = AskSaveOnClose;
            localSettings.Values[Settings_Magnifier_Radius] = MagnifierRadius;
            localSettings.Values[Settings_Magnifier_Scale] = MagnifierScale;
            localSettings.Values[Settings_Magnifier_Crosshairs] = MagnifierShowCrosshairs;
        }

        public event Action ThemeChanged;

        private AppSettings() { }

        #region settings

        private bool _AskSaveOnClose = false;
        public bool AskSaveOnClose
        {
            get => this._AskSaveOnClose;
            set => SetProperty(ref _AskSaveOnClose, value, onChanged: () =>
            {
                ApplicationData.Current.LocalSettings.Values[Settings_AskSaveOnClose] = value;
            });
        }
        private ElementTheme _Theme = ElementTheme.Default;
        public ElementTheme Theme
        {
            get => _Theme;
            set
            {
                if ((int)value == -1)
                    return;
                SetProperty(ref _Theme, value, onChanged: () =>
                {
                    ApplicationData.Current.LocalSettings.Values[Settings_Theme] = (int)value;
                });
                ThemeChanged?.Invoke();
            }
        }

        private int _MagnifierRadius = 50;
        public int MagnifierRadius
        {
            get => _MagnifierRadius;
            set
            {
                SetProperty(ref _MagnifierRadius, value, onChanged: () =>
                {
                    ApplicationData.Current.LocalSettings.Values[Settings_Magnifier_Radius] = value;
                });
            }
        }

        private int _MagnifierScale = 4;
        public int MagnifierScale
        {
            get => _MagnifierScale;
            set
            {
                SetProperty(ref _MagnifierScale, value, onChanged: () =>
                {
                    ApplicationData.Current.LocalSettings.Values[Settings_Magnifier_Scale] = value;
                });
            }
        }

        private bool _MagnifierShowCrosshairs = true;
        public bool MagnifierShowCrosshairs
        {
            get => _MagnifierShowCrosshairs;
            set
            {
                SetProperty(ref _MagnifierShowCrosshairs, value, onChanged: () =>
                {
                    ApplicationData.Current.LocalSettings.Values[Settings_Magnifier_Crosshairs] = value;
                });
            }
        }
        #endregion
    }
}
