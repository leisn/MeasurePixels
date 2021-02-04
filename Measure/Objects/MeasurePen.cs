using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI;

namespace MeasurePixels.Measure.Objects
{
    public struct MeasurePen
    {
        public int Width { get; set; }
        public Color Color { get; set; }
        public float FontSize { get; set; }
        public string FontFamily { get; set; }
        public Color TextColor { get; set; }
        public Color TextBackground { get; set; }
        public int GlyphSize { get; set; }

        public static MeasurePen Default => new MeasurePen
        {
            FontFamily= Windows.UI.Xaml.Media.FontFamily.XamlAutoFontFamily.Source,
            Width = 2,
            Color = Colors.Black,
            FontSize = 14f,
            TextColor = Colors.Black,
            TextBackground = (Color)App.Current.Resources["SystemAccentColorLight1"],
            GlyphSize = 10
        };

        public MeasurePen Set(string key,object value)
        {
            switch (key)
            {
                case nameof(Width):
                    Width = (int)value;
                    break;
                case nameof(Color):
                    Color = parseColor(value);
                    break;
                case nameof(FontSize):
                    FontSize = (float)value;
                    break;
                case nameof(FontFamily):
                    FontFamily = value?.ToString();
                    break;
                case nameof(TextColor):
                    TextColor = parseColor(value);
                    break;
                case nameof(TextBackground):
                    TextBackground = parseColor(value);
                    break;
                case nameof(GlyphSize):
                    GlyphSize = (int)value;
                    break;
            }
            return this;
        }

        private Color parseColor(object value)
        {
            if (value is string)
                return ColorExts.FromHex(value.ToString());
            else
                return (Color)value;
        }
    }
}
