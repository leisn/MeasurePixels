using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MeasurePixels.Helpers
{
    public class BoolVisibleReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (bool)value;
            return val ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var val = (Visibility)value;
            return val == Visibility.Collapsed;
        }
    }
}
