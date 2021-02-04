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
    public class EnumBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (int)value;
            var par = int.Parse(parameter?.ToString());
            return val == par;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var val = (bool)value;
            return val ? int.Parse((string)parameter) : -1;
        }
    }
}
