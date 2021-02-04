using System;

using Microsoft.Graphics.Canvas;

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MeasurePixels.Measure
{
    public interface IMeasureCanvas
    {
        CanvasBitmap SourceBitmap { get; }
        FontFamily FontFamily { get; }
        ICanvasResourceCreator Creator { get; }
        Color SelectedBorderColor { get; }
        float SelectedBorderWidth { get; }
        int StartX { get; }
        int StartY { get; }
    }
}
