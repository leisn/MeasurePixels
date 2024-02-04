using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using MeasurePixels.Measure.Objects;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeasurePixels.Measure
{
    public partial class MeasureCanvas
    {
        readonly List<MeasureObject> measureObjects = new List<MeasureObject>();
        CanvasBitmap orginBitmap;
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
            this.InitCanvasClearColor();
            if (orginBitmap == null) return;
            var contentSize = orginBitmap.SizeInPixels;
            var width = (int)contentSize.Width + StartX * 2;
            var height = (int)contentSize.Height + StartY * 2;
            measureBitmap = CreateEmptyBitmap(width, height);
            measureBitmap.CopyPixelsFromBitmap(orginBitmap, StartX, StartY);
            magnifier.Source = measureBitmap;
            this.canvas.Invalidate();
        }

        public void Update(CanvasBitmap bitmap, bool redraw = true)
        {
            Clear();
            orginBitmap?.Dispose();
            orginBitmap = bitmap;
            if (bitmap == null)
            {
                this.measureBitmap.Dispose();
                this.measureBitmap = null;
                this.canvas.Width = this.canvas.Height = 0;
                CanMeasure = false;
                return;
            }

            var contentSize = orginBitmap.SizeInPixels;
            var width = (int)contentSize.Width + StartX * 2;
            var height = (int)contentSize.Height + StartY * 2;
            measureBitmap = CreateEmptyBitmap(width, height);
            measureBitmap.CopyPixelsFromBitmap(orginBitmap, StartX, StartY);

            this.canvas.Width = width;
            this.canvas.Height = height;

            magnifier = magnifier ?? new Magnifier(this.canvas);
            magnifier.Source = measureBitmap;

            if (redraw) this.canvas.Invalidate();
            CanMeasure = true;
        }

        private void UpdateMagnifier(Point center)
        {
            if (magnifier != null)
            {
                magnifier.CenterX = (float)center.X;
                magnifier.CenterY = (float)center.Y;
            }

            var x = (int)(center.X - StartX);
            var y = (int)(center.Y - StartY);
            this.coordText.Text = $"{x}, {y}";
            x = (int)Math.Ceiling(center.X);
            y = (int)Math.Ceiling(center.Y);
            if (measureBitmap != null && x >= 0 && x < measureBitmap.Bounds.Width && y >= 0 && y < measureBitmap.Bounds.Height)
            {
                var color = measureBitmap?.GetPixelColors(x, y, 1, 1)?.FirstOrDefault();
                if (color.HasValue)
                {
                    this.colorText.Text = $"Rgb({color.Value.R}, {color.Value.G}, {color.Value.B})";
                    //  this.colorText.Text = color.Value.ToHex();
                }
            }
        }

        float _scale = 1;
        private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (measureBitmap == null)
                return;
            var session = args.DrawingSession;
            if (IsOjbectsVisible != null)
                session.DrawImage(measureBitmap);
            else
                session.DrawRectangle(measureBitmap.Bounds, Colors.Transparent);
            session.Antialiasing = CanvasAntialiasing.Antialiased;
            session.TextAntialiasing = CanvasTextAntialiasing.Auto;

            if (IsOjbectsVisible != false)
                for (int i = 0; i < undoIndex + 1; i++)
                    measureObjects[i].Draw(session);

            if (IsMagnifierVisible)
            {
                magnifier.Scale = settings.MagnifierScale;
                magnifier.Radius = settings.MagnifierRadius;
                magnifier.ShowCrosshairs = settings.MagnifierShowCrosshairs;
                magnifier.Draw(session, new Size(sender.Width, sender.Height));
            }

            temp?.Draw(session);
        }
    }
}
