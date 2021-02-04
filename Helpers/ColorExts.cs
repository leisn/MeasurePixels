using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI;

namespace MeasurePixels
{
    public static class ColorExts
    {
        public static string ToHex(this Color color)
        {
            var str = "#";
            if (color.A != 0xFF)
                str += color.A.ToString("X2");
            str += color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            return str;
        }

        public static Color Inverse(this Color self)
        {
            var r = 255 - self.R;
            var g = 255 - self.G;
            var b = 255 - self.B;
            return Color.FromArgb(self.A, (byte)r, (byte)g, (byte)b);
        }

        public static Color GetSimpleForeground(this Color self)
        {
            var l = (0.299 * self.R + 0.587 * self.G + 0.114 * self.B) / 255;
            return l > 0.5 ? Colors.Black : Colors.White;
        }

        public static Color FromHex(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException("色值=null");
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);
            byte a = 0xFF, r, g, b;
            if (hex.Length == 3)
            {
                var temp = hex.Substring(0, 1);
                temp += temp;
                r = byte.Parse(temp, NumberStyles.HexNumber);
                temp = hex.Substring(1, 1);
                temp += temp;
                g = byte.Parse(temp, NumberStyles.HexNumber);
                temp = hex.Substring(2, 1);
                temp += temp;
                b = byte.Parse(temp, NumberStyles.HexNumber);
            }
            else if (hex.Length == 4)
            {
                var temp = hex.Substring(0, 1);
                temp += temp;
                a = byte.Parse(temp, NumberStyles.HexNumber);
                temp = hex.Substring(1, 1);
                temp += temp;
                r = byte.Parse(temp, NumberStyles.HexNumber);
                temp = hex.Substring(2, 1);
                temp += temp;
                g = byte.Parse(temp, NumberStyles.HexNumber);
                temp = hex.Substring(3, 1);
                temp += temp;
                b = byte.Parse(temp, NumberStyles.HexNumber);
            }
            else if (hex.Length == 6)
            {
                r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            }
            else if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
            }
            else
            {
                throw new ArgumentException("色值格式不正确");
            }
            return Color.FromArgb(a, r, g, b);
        }
    }
}
