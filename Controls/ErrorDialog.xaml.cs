using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MeasurePixels.Controls
{
    public sealed partial class ErrorDialog : ContentDialog
    {
        public ErrorDialog()
        {
            this.InitializeComponent();
        }

        public Exception ErrorException
        {
            get { return (Exception)GetValue(ErrorExceptionProperty); }
            set { SetValue(ErrorExceptionProperty, value); }
        }

        public static readonly DependencyProperty ErrorExceptionProperty =
          DependencyProperty.Register(
              nameof(ErrorException),
              typeof(Exception),
              typeof(ErrorDialog),
              new PropertyMetadata(null));

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var str = ErrorException?.ToString();
            str += ErrorException?.TargetSite?.DeclaringType 
                + "::" + ErrorException?.TargetSite?.Name;
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(str);
            Clipboard.SetContent(dataPackage);
        }
    }
}
