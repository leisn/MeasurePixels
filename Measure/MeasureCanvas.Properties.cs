using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeasurePixels.Measure.Objects;

using Microsoft.Graphics.Canvas;

using Windows.UI;
using Windows.UI.Xaml;

namespace MeasurePixels.Measure
{
    public partial class MeasureCanvas : IMeasureCanvas
    {
        public event EventHandler<MeasureObject> OnSelectedChanged;
        public ICanvasResourceCreator Creator => this.canvas;
        public CanvasDevice Device => this.canvas.Device;
        public CanvasBitmap SourceBitmap => this.measureBitmap;
        public Color SelectedBorderColor => (Color)App.Current.Resources["SystemAccentColorDark1"];
        public float SelectedBorderWidth => 1.0f;

        public readonly Dictionary<MeasureTool, MeasurePen>
            Pens = new Dictionary<MeasureTool, MeasurePen> {
                {MeasureTool.Segment,MeasurePen.Default.Set("GlyphSize",15) },
                {MeasureTool.Angle,MeasurePen.Default.Set("GlyphSize",20) },
                {MeasureTool.Edge,MeasurePen.Default.Set("GlyphSize",10) },
                {MeasureTool.Eyedropper,MeasurePen.Default.Set("GlyphSize",16) },
            };

        public MeasureTool ActiveTool
        {
            get { return (MeasureTool)GetValue(ActiveToolProperty); }
            set
            {
                if ((int)value == -1)
                    return;
                SetValue(ActiveToolProperty, value);
            }
        }

        public static readonly DependencyProperty ActiveToolProperty =
          DependencyProperty.Register(
              nameof(ActiveTool),
              typeof(MeasureTool),
              typeof(MeasureCanvas),
              new PropertyMetadata(MeasureTool.Pointer));

        public float DpiScale
        {
            get { return (float)GetValue(DpiScaleProperty); }
            set { SetValue(DpiScaleProperty, value); }
        }

        public static readonly DependencyProperty DpiScaleProperty =
            DependencyProperty.Register(
                nameof(DpiScale),
                typeof(float),
                typeof(MeasureCanvas),
                new PropertyMetadata(1f));

        public bool CanMeasure
        {
            get { return (bool)GetValue(CanMeasureProperty); }
            set { SetValue(CanMeasureProperty, value); }
        }
        public static readonly DependencyProperty CanMeasureProperty =
            DependencyProperty.Register(
                nameof(CanMeasure),
                typeof(bool),
                typeof(MeasureCanvas),
                new PropertyMetadata(false));

        public bool CanUndo
        {
            get { return (bool)GetValue(CanUndoProperty); }
            set { SetValue(CanUndoProperty, value); }
        }
        public static readonly DependencyProperty CanUndoProperty =
            DependencyProperty.Register(
                nameof(CanUndo),
                typeof(bool),
                typeof(MeasureCanvas),
                new PropertyMetadata(false));

        public bool CanRedo
        {
            get { return (bool)GetValue(CanRedoProperty); }
            set { SetValue(CanRedoProperty, value); }
        }
        public static readonly DependencyProperty CanRedoProperty =
            DependencyProperty.Register(
                nameof(CanRedo),
                typeof(bool),
                typeof(MeasureCanvas),
                new PropertyMetadata(false));

        public bool? IsOjbectsVisible
        {
            get { return (bool?)GetValue(IsOjbectsVisibleProperty); }
            set { SetValue(IsOjbectsVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsOjbectsVisibleProperty =
            DependencyProperty.Register(
                nameof(IsOjbectsVisible),
                typeof(bool?),
                typeof(MeasureCanvas),
                new PropertyMetadata(true, CanvasPropertyChanged));

        public bool IsMagnifierVisible
        {
            get { return (bool)GetValue(IsMagnifierVisibleProperty); }
            set { SetValue(IsMagnifierVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsMagnifierVisibleProperty =
            DependencyProperty.Register(
                nameof(IsMagnifierVisible),
                typeof(bool),
                typeof(MeasureCanvas),
                new PropertyMetadata(true, CanvasPropertyChanged));

        public MeasureObject SelectedItem
        {
            get { return (MeasureObject)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(MeasureObject),
                typeof(MeasureCanvas),
                new PropertyMetadata(null, SelectedChangedCallback));

        private static void SelectedChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var old = e.OldValue as MeasureObject;
            if (old != null)
                old.IsSelected = false;
            var ne = e.NewValue as MeasureObject;
            if (ne != null)
                ne.IsSelected = true;
            var mc = d as MeasureCanvas;
            if (mc != null)
            {
                mc.canvas?.Invalidate();
                mc.OnSelectedChanged?.Invoke(mc, ne);
            }
        }

        private static void CanvasPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MeasureCanvas)?.canvas?.Invalidate();
        }
    }
}
