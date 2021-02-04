using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeasurePixels.Measure.Objects;

using Microsoft.Graphics.Canvas.Text;

using Windows.UI;

namespace MeasurePixels.ViewModels
{
    public class MeasurePenViewModel : BaseViewModel
    {
        public static string[] ColorDefines = new string[]
        {
            "#000000","#FFFFFF","#D1D3D4","#A7A9AC","#808285","#58595B",
            "#B31564","#E61B1B","#FF5500","#A7A9AC","#FFAA00","#FFCE00",
            "#A2E61B","#26E600","#008055","#00AACC","#004DE6","#3D00B8",
            "#6600CC","#600080","#F7D7C4","#BB9167","#8E562E","#613D30",
            "#FF80FF","#FFC680","#FFFF80","#80FF9E","#80D6FF","#BCB3FF"
        };
        public ObservableCollection<string> Fonts { get; }

        public string[] Colors => ColorDefines;

        public MeasurePenViewModel()
        {
            Fonts = new ObservableCollection<string>();
            LoadFonts();
        }

        public event Action OnChanged;

        public Action OnGlyphChanged;

        private int glyphSize=10;
        public int GlyphSize
        {
            get => glyphSize;
            set
            {
                SetProperty(ref glyphSize, value, onChanged: OnGlyphChanged);
                OnChanged?.Invoke();
            }
        }
        private int penWidth=2;
        public int PenWidth
        {
            get => penWidth;
            set
            {
                SetProperty(ref penWidth, value, onChanged: OnGlyphChanged);
                OnChanged?.Invoke();
            }
        }

        private string selectedColor="#000";
        public string SelectedColor
        {
            get => selectedColor;
            set
            {
                SetProperty(ref selectedColor, value, onChanged: OnGlyphChanged);
                OnChanged?.Invoke();
            }
        }

        private string fontName;
        public string FontName
        {
            get => fontName;
            set => SetProperty(ref fontName, value, onChanged: OnChanged);
        }

        private Color textColor;
        public Color TextColor
        {
            get => textColor;
            set => SetProperty(ref textColor, value, onChanged: OnChanged);
        }

        private Color textBg;
        public Color TextBg
        {
            get => textBg;
            set => SetProperty(ref textBg, value, onChanged: OnChanged);
        }

        private int fontSize;
        public int FontSize
        {
            get => fontSize;
            set => SetProperty(ref fontSize, value, onChanged: OnChanged);
        }

        private MeasurePen pen;
        public void SyncPen(MeasurePen pen)
        {
            this.pen = pen;
            this.GlyphSize = pen.GlyphSize;
            this.PenWidth = pen.Width;
            this.SelectedColor = pen.Color.ToHex();
            this.FontName = pen.FontFamily;
            this.TextColor = pen.TextColor;
            this.TextBg = pen.TextBackground;
            this.FontSize = (int)pen.FontSize;
        }


        public MeasurePen Pen
        {
            get
            {
                pen.Color = ColorExts.FromHex(SelectedColor);
                pen.Width = PenWidth;
                pen.GlyphSize = GlyphSize;
                pen.FontFamily = FontName;
                pen.TextBackground = TextBg;
                pen.TextColor = TextColor;
                pen.FontSize = FontSize;
                return pen;
            }
        }

        public void LoadFonts()
        {
            var items = CanvasTextFormat.GetSystemFontFamilies();
            foreach (var item in items)
                Fonts.Add(item);
        }
    }
}
