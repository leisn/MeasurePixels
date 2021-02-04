using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
namespace MeasurePixels.Helpers
{
    public class CanvasSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (double)value;
            var param = parameter?.ToString();
            return param == "width" ? val + 150 : param == "height" ? val + 140 : val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
