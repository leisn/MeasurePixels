using System.Collections.Generic;

using MeasurePixels.Measure.Objects;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;

namespace MeasurePixels.Measure
{
    public partial class MeasureCanvas
    {
        readonly List<MeasureObject> measureObjects = new List<MeasureObject>();
        CanvasBitmap measureBitmap;
        MeasureObject temp;
        Magnifier magnifier;
        AppSettings settings => AppSettings.Current;
        public int StartX { get; } = 60;
        public int StartY { get; } = 50;


        private CanvasBitmap CreateEmptyBitmap(int width, int height, CanvasControl canvasControl = null)
        {
            canvasControl = canvasControl ?? this.canvas;
            var colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = canvasControl.ClearColor;
            var bitmap = CanvasBitmap.CreateFromColors(canvasControl, colors, width, height);
            return bitmap;
        }


        private void Settings_ThemeChanged()
        {
            this.initCavansClearColor();
            if (measureBitmap == null) return;
            var size = measureBitmap.SizeInPixels;
            var bitmap = CreateEmptyBitmap((int)size.Width, (int)size.Height);
            bitmap.CopyPixelsFromBitmap(measureBitmap, StartX, StartY,
                StartX, StartY, (int)size.Width - StartX * 2, (int)size.Height - StartY * 2);
            measureBitmap = bitmap;
            magnifier.Source = measureBitmap;
            //this.canvas.Invalidate();
        }

        public void Update(CanvasBitmap bitmap, bool redraw = true)
        {
            Clear();

            if (bitmap == null)
            {
                this.measureBitmap.Dispose();
                this.measureBitmap = null;
                this.canvas.Width = this.canvas.Height = 0;
                CanMeasure = false;
                return;
            }

            var contentSize = bitmap.SizeInPixels;
            var width = (int)contentSize.Width + StartX * 2;
            var height = (int)contentSize.Height + StartY * 2;
            measureBitmap = CreateEmptyBitmap(width, height);
            measureBitmap.CopyPixelsFromBitmap(bitmap, StartX, StartY);

            this.canvas.Width = width;
            this.canvas.Height = height;

            magnifier = magnifier ?? new Magnifier(this.canvas);
            magnifier.Source = measureBitmap;

            if (redraw)
                this.canvas.Invalidate();
            CanMeasure = true;
        }


        private void UpdateMagnifier(Point center)
        {
            if (magnifier != null)
            {
                magnifier.CenterX = (float)center.X;
                magnifier.CenterY = (float)center.Y;
            }

            this.coordText.Text = (center.X - StartX).ToString("0") + ", " + (center.Y - StartY).ToString("0");
        }

        private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (measureBitmap == null)
                return;
            var session = args.DrawingSession;
            session.DrawImage(measureBitmap);
            session.Antialiasing = CanvasAntialiasing.Antialiased;
            session.TextAntialiasing = CanvasTextAntialiasing.Auto;

            if (IsOjbectsVisible)
                for (int i = 0; i < undoIndex + 1; i++)
                    measureObjects[i].Draw(session);

            if (IsMagnifierVisible)
            {
                magnifier.Scale = settings.MagnifierScale;
                magnifier.Radius = settings.MagnifierRadius;
                magnifier.Draw(session, new Size(sender.Width, sender.Height));
            }

            temp?.Draw(session);
        }
    }
}
