using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using MeasurePixels.Controls;
using MeasurePixels.Measure.Objects;
using MeasurePixels.ViewModels;

using Windows.ApplicationModel.DataTransfer;
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
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MeasurePixels.Measure
{
    public sealed partial class MeasureDetail : UserControl
    {

        private MeasureDetailViewModel viewModel;


        public MeasureDetail()
        {
            this.InitializeComponent();
            viewModel = new MeasureDetailViewModel();
            viewModel.OnAddTextBlock += ViewModel_OnAddTextBlock;
        }

        private void ViewModel_OnAddTextBlock(object sender, IndexControl e)
        {
            this.detailCanvas.Children.Add(e);
        }

        public void SetObject(MeasureObject obj)
        {
            if (obj != null)
                viewModel.UpdateValues(obj);
        }

        private async void PropertyItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var item = (PropertyItem)e.ClickedItem;
                DataPackage dataPackage = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                dataPackage.SetText(item.Value);
                Clipboard.SetContent(dataPackage);
                await Helpers.Toast.ShowAsync("已复制");
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = ex.GetType(),
                    Content = ex.Message,
                    PrimaryButtonText = "OK"
                };
                await dialog.ShowAsync();
            }
        }
    }
}
