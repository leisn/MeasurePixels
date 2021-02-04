using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using MeasurePixels.Measure.Objects;
using MeasurePixels.ViewModels;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MeasurePixels.Measure
{
    public sealed partial class MeasurePenConf : UserControl
    {

        private MeasureTool toolType;

        public MeasurePenViewModel viewModel;
        private int GlyphSize => viewModel.GlyphSize;

        public event Action<MeasureTool, MeasurePen> OnPenChanged;

        public MeasurePenConf()
        {
            this.InitializeComponent();
            viewModel = new MeasurePenViewModel();
            viewModel.OnChanged += OnChanged;
            viewModel.OnGlyphChanged = UpdateGlyph;
        }

        private void OnChanged() => OnPenChanged?.Invoke(toolType, viewModel.Pen);

        public void SetPen(MeasureTool tool, MeasurePen pen)
        {
            this.toolType = tool;
            viewModel.SyncPen(pen);
        }

        private void UpdateGlyph()
        {
            switch (toolType)
            {
                case MeasureTool.Segment:
                    UpdateSegemnt();
                    break;
                case MeasureTool.Angle:
                    UpdateAngle();
                    break;
                case MeasureTool.Edge:
                    UpdateEdge();
                    break;
                case MeasureTool.Eyedropper:
                    UpdateColor();
                    break;
                case MeasureTool.Pointer:
                default:
                    break;
            }
        }
        private int canvasWidth = 110;
        private int canvasHeight = 60;
        private void UpdateSegemnt()
        {
            var glyph = new GeometryGroup();
            var top = Math.Ceiling(canvasHeight / 2d);
            var line = new LineGeometry();
            line.StartPoint = new Point(0, top);
            line.EndPoint = new Point(canvasWidth, top);
            glyph.Children.Add(line);

            if (GlyphSize > 0)
            {
                var gl = new LineGeometry();
                top = Math.Ceiling((canvasHeight - GlyphSize) / 2d);

                gl.StartPoint = new Point(0, top);
                gl.EndPoint = new Point(0, top + GlyphSize);
                glyph.Children.Add(gl);

                gl = new LineGeometry();
                gl.StartPoint = new Point(canvasWidth, top);
                gl.EndPoint = new Point(canvasWidth, top + GlyphSize);
                glyph.Children.Add(gl);
            }
            glyphPath.Data = glyph;
        }
        private void UpdateAngle()
        {
            var glyph = new GeometryGroup();
            var pG = new PathGeometry();
            var fC = new PathFigureCollection();
            var figure = new PathFigure
            {
                IsClosed = false,
                StartPoint = new Point(canvasHeight, 0)
            };
            figure.Segments.Add(new LineSegment
            {
                Point = new Point(0, canvasHeight)
            });
            figure.Segments.Add(new LineSegment
            {
                Point = new Point(canvasWidth, canvasHeight)
            });
            fC.Add(figure);
            pG.Figures = fC;
            glyph.Children.Add(pG);

            if (GlyphSize > 0)
            {
                pG = new PathGeometry();
                fC = new PathFigureCollection();
                figure = new PathFigure
                {
                    IsClosed = false,
                    StartPoint = new Point(GlyphSize, canvasHeight)
                };
                var gw = GlyphSize / Math.Sqrt(2);
                figure.Segments.Add(new ArcSegment
                {
                    Size = new Size(GlyphSize, GlyphSize),
                    SweepDirection = SweepDirection.Counterclockwise,
                    RotationAngle = 45,
                    Point = new Point(gw, canvasHeight - gw)
                });
                fC.Add(figure);
                pG.Figures = fC;
                glyph.Children.Add(pG);
            }
            glyphPath.Data = glyph;
        }
        private void UpdateEdge()
        {
            var glyph = new GeometryGroup();
            var rect = new RectangleGeometry
            {
                Rect = new Rect(0, 0, canvasWidth, canvasHeight)
            };
            glyph.Children.Add(rect);

            if (GlyphSize > 0)
            {
                var pG = new PathGeometry();
                var fC = new PathFigureCollection();
                var figure = new PathFigure
                {
                    IsClosed = false,
                    StartPoint = new Point(GlyphSize, canvasHeight)
                };
                figure.Segments.Add(new LineSegment
                {
                    Point = new Point(GlyphSize, canvasHeight - GlyphSize)
                });
                figure.Segments.Add(new LineSegment
                {
                    Point = new Point(0, canvasHeight - GlyphSize)
                });
                fC.Add(figure);
                pG.Figures = fC;
                glyph.Children.Add(pG);
            }

            glyphPath.Data = glyph;
        }
        private void UpdateColor()
        {
            var glyph = new GeometryGroup();

            var height = viewModel.PenWidth * 2 + 5 + GlyphSize;
            var rect = new Rect((canvasWidth - GlyphSize) / 2,
                (canvasHeight - height) / 2, 1, 1);
            glyph.Children.Add(new RectangleGeometry
            {
                Rect = rect
            });
            glyph.Children.Add(new RectangleGeometry
            {
                Rect = new Rect((canvasWidth - GlyphSize) / 2,
               rect.Bottom + viewModel.PenWidth + 5, GlyphSize, GlyphSize)
            });
            glyphPath.Data = glyph;
        }



        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedIndex),
                typeof(int),
                typeof(MeasurePenConf),
                new PropertyMetadata(0));

        private void penTabHeader_Click(object sender, RoutedEventArgs e)
        {
            penTab.Visibility = Visibility.Visible;
            textTab.Visibility = Visibility.Collapsed;
        }

        private void textTabHeader_Click(object sender, RoutedEventArgs e)
        {
            penTab.Visibility = Visibility.Collapsed;
            textTab.Visibility = Visibility.Visible;
        }
    }
}
