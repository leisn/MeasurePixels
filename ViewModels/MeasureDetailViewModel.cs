using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeasurePixels.Controls;
using MeasurePixels.Measure.Objects;

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MeasurePixels.ViewModels
{
    public class PropertyItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public Color HexColor { get; set; } = Colors.Transparent;
        public bool IsColorSet { get; set; }
    }

    public class MeasureDetailViewModel : BaseViewModel
    {
        public int CanvasWidth { get; } = 180;
        public int CanvasHeight { get; } = 100;

        private int indexControlSize = 20;
        private int padding = 10;


        public readonly List<IndexControl> Controls = new List<IndexControl>();

        public ObservableCollection<PropertyItem> Items { get; }

        public PointCollection Points { get; }

        public event EventHandler<IndexControl> OnAddTextBlock;

        public MeasureDetailViewModel()
        {
            Items = new ObservableCollection<PropertyItem>();
            Points = new PointCollection();
        }

        public void UpdateValues(MeasureObject mo)
        {
            Items.Clear();
            Points.Clear();
            foreach (var item in Controls)
                item.Visibility = Visibility.Collapsed;
            if (mo == null) return;
            var type = mo.GetType();
            var list = new List<Point>();
            if (type == typeof(MeasureSegment))
            {
                var sege = (MeasureSegment)mo;
                Items.Add(new PropertyItem { Key = "|1-2|", Value = sege.Distance.Round3().ToString() });
                Items.Add(new PropertyItem { Key = "∠ 1", Value = sege.Angle.Round3() + "°" });
                list.Add(sege.ClipStarts(sege.X1, sege.Y1));
                list.Add(sege.ClipStarts(sege.X2, sege.Y2));
                AddScaledPoints(list, false);
            }
            else if (type == typeof(MeasureAngle))
            {
                var sege = (MeasureAngle)mo;
                list.Add(sege.ClipStarts(sege.X1, sege.Y1));
                list.Add(sege.ClipStarts(sege.X2, sege.Y2));
                list.Add(sege.ClipStarts(sege.X3, sege.Y3));
                list.Add(sege.ClipStarts(sege.X1, sege.Y1));//闭合
                AddScaledPoints(list, true);
                Items.Add(new PropertyItem { Key = "|1-2|", Value = sege.Distance12.Round3().ToString() });
                Items.Add(new PropertyItem { Key = "|2-3|", Value = sege.Distance23.Round3().ToString() });
                Items.Add(new PropertyItem { Key = "|1-3|", Value = sege.Distance13.Round3().ToString() });
                Items.Add(new PropertyItem { Key = "∠ 1", Value = sege.Angle213.Round3() + "°" });
                Items.Add(new PropertyItem { Key = "∠ 2", Value = sege.Angle123.Round3() + "°" });
                Items.Add(new PropertyItem { Key = "∠ 3", Value = sege.Angle132.Round3() + "°" });
            }
            else if (type == typeof(MeasureRect))
            {
                var sege = (MeasureRect)mo;
                list.Add(sege.ClipStarts(sege.P11.X, sege.P11.Y));
                list.Add(sege.ClipStarts(sege.P12.X, sege.P12.Y));
                list.Add(sege.ClipStarts(sege.P22.X, sege.P22.Y));
                list.Add(sege.ClipStarts(sege.P21.X, sege.P21.Y));
                list.Add(sege.ClipStarts(sege.P11.X, sege.P11.Y));//闭合
                AddScaledPoints(list, true);
                Items.Add(new PropertyItem { Key = "|1-2|", Value = sege.Distance12.Round3().ToString() });
                Items.Add(new PropertyItem { Key = "|1-3|", Value = sege.Distance13.Round3().ToString() });
                Items.Add(new PropertyItem { Key = "|2-3|", Value = sege.Distance23.Round3().ToString() });
                Items.Add(new PropertyItem { Key = "|2-4|", Value = sege.Distance24.Round3().ToString() });
                Items.Add(new PropertyItem { Key = "|3-4|", Value = sege.Distance34.Round3().ToString() });
                Items.Add(new PropertyItem { Key = "|4-1|", Value = sege.Distance14.Round3().ToString() });

                Items.Add(new PropertyItem { Key = "∠ 1", Value = sege.Angle1.Round3() + "°" });
                Items.Add(new PropertyItem { Key = "∠ 2", Value = sege.Angle2.Round3() + "°" });
                Items.Add(new PropertyItem { Key = "∠ 3", Value = sege.Angle3.Round3() + "°" });
                Items.Add(new PropertyItem { Key = "∠ 4", Value = sege.Angle4.Round3() + "°" });
                Items.Add(new PropertyItem { Key = "∠ 124", Value = sege.Angle124.Round3() + "°" });
                Items.Add(new PropertyItem { Key = "∠ 231", Value = sege.Angle231.Round3() + "°" });
                Items.Add(new PropertyItem { Key = "∠ 342", Value = sege.Angle342.Round3() + "°" });
                Items.Add(new PropertyItem { Key = "∠ 413", Value = sege.Angle413.Round3() + "°" });
            }
            else if (type == typeof(MeasureColor))
            {
                var sege = (MeasureColor)mo;
                UpdateForMeasureColor(sege);
            }
        }

        private void UpdateForMeasureColor(MeasureColor mc)
        {
            var canvasWidth = CanvasWidth - padding * 2;
            var canvasHeight = CanvasHeight - padding * 2;
            var p = mc.ClipStarts(mc.X, mc.Y);
            var x = (int)(p.X - 1);
            var y = (int)(p.Y - 1);
            var width = indexControlSize * 3 + 5 * 2;
            var left = (canvasWidth - width) / 2;
            var top = (canvasHeight - width) / 2;
            for (int i = 0; i < 9; i++)
            {
                int row = i / 3;
                int col = i % 3;
                var textBlock = getOrCreateBlock(i);
                textBlock.Visibility = Visibility.Visible;
                var c = mc.Colors[i];
                var cf = c.GetSimpleForeground();
                (textBlock.Background as SolidColorBrush).Color = c;
                (textBlock.Foreground as SolidColorBrush).Color = cf;
                if (i == 4)
                    textBlock.CornerRadius = new CornerRadius(0);
                textBlock.SetValue(ToolTipService.ToolTipProperty, (x + col) + ", " + (y + row));
                var cl = left + col * (indexControlSize + 5);
                var ct = top + row * (indexControlSize + 5);
                textBlock.SetValue(Canvas.LeftProperty, cl);
                textBlock.SetValue(Canvas.TopProperty, ct);
                Items.Add(new PropertyItem
                {
                    Key =  (i + 1).ToString(),
                    Value = c.ToHex(),
                    HexColor = c,
                    IsColorSet = true
                }); ;
            }
        }

        private void AddScaledPoints(List<Point> points, bool ignoreLastPoint)
        {
            var leftMost = double.PositiveInfinity;
            var topMost = double.PositiveInfinity;
            var rightMost = double.NegativeInfinity;
            var bottomMost = double.NegativeInfinity;

            foreach (var item in points)
            {
                leftMost = Math.Min(item.X, leftMost);
                topMost = Math.Min(item.Y, topMost);
                bottomMost = Math.Max(item.Y, bottomMost);
                rightMost = Math.Max(item.X, rightMost);
            }

            var canvasWidth = CanvasWidth - padding * 2;
            var canvasHeight = CanvasHeight - padding * 2;

            var width = rightMost - leftMost;
            var height = bottomMost - topMost;
            double scaleX = 1;
            double scaleY = 1;
            if (width > canvasWidth)
                scaleX = canvasWidth / width;
            if (height > canvasHeight)
                scaleY = canvasHeight / height;
            var scale = Math.Min(scaleX, scaleY);

            var scaledLeftMost = leftMost * scale;
            var scaledTopMost = topMost * scale;
            var scaledBottomMost = bottomMost * scale;
            var scaledRightMost = rightMost * scale;

            width = scaledRightMost - scaledLeftMost;
            height = scaledBottomMost - scaledTopMost;

            var offsetX = (canvasWidth - width) / 2 - scaledLeftMost + padding;
            var offsetY = (canvasHeight - height) / 2 - scaledTopMost + padding;

            for (int i = 0; i < points.Count; i++)
            {
                var item = points[i];
                var x = item.X * scale + offsetX;
                var y = item.Y * scale + offsetY;
                var p = new Point(x, y);
                Points.Add(p);
                if (ignoreLastPoint && i == points.Count - 1)
                    break;
                var textBlock = getOrCreateBlock(i);
                (textBlock.Background as SolidColorBrush).Color = Colors.White;
                (textBlock.Foreground as SolidColorBrush).Color = Colors.Black;
                textBlock.CornerRadius = new CornerRadius(9);
                textBlock.Visibility = Visibility.Visible;
                textBlock.SetValue(ToolTipService.ToolTipProperty,
                    item.X.Round3().ToString() + ", " + item.Y.Round3().ToString());
                var size = textBlock.DesiredSize;
                textBlock.SetValue(Canvas.LeftProperty, x - indexControlSize / 2);
                textBlock.SetValue(Canvas.TopProperty, y - indexControlSize / 2);
            }
            var count = ignoreLastPoint ? points.Count - 1 : points.Count;
            for (int i = count; i < Controls.Count; i++)
                Controls[i].Visibility = Visibility.Collapsed;
        }

        private IndexControl getOrCreateBlock(int index)
        {
            if (index < Controls.Count)
                return Controls[index];
            var textBlock = new IndexControl();
            //textBlock.FontSize = 12;
            textBlock.Text = (index + 1).ToString();
            textBlock.Foreground = new SolidColorBrush(Colors.Black);
            textBlock.Background = new SolidColorBrush(Colors.White);
            Controls.Add(textBlock);
            OnAddTextBlock?.Invoke(this, textBlock);
            return textBlock;
        }
    }
}
