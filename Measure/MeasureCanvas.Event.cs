using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using MeasurePixels.Measure.Objects;

using Microsoft.Graphics.Canvas;

using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace MeasurePixels.Measure
{
    public partial class MeasureCanvas
    {
        #region mouse handler
        readonly Stopwatch watch = new Stopwatch();
        private void canvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            //只是处理左右平移
            if (e.KeyModifiers == VirtualKeyModifiers.Shift)
            {
                var delta = e.GetCurrentPoint(sender as UIElement).Properties.MouseWheelDelta;
                delta = delta > 0 ? 1 : -1;
                scroll.ChangeView(scroll.HorizontalOffset - delta * scroll.ScrollableWidth / 9,
                    scroll.VerticalOffset, scroll.ZoomFactor);
                e.Handled = true;
                return;
            }
            //缩放
            if (e.KeyModifiers == VirtualKeyModifiers.Control)
            {
                var pointer = e.GetCurrentPoint(sender as UIElement);
                bool zoomIn = pointer.Properties.MouseWheelDelta > 0;
                var scale = (float)Math.Clamp(_scale + (zoomIn ? 0.05 : -0.05), 0.1, 4);
                //if (scale != _scale)
                //{
                //    _scalePoint = pointer.Position;
                //    _scale = scale;
                //    canvas.Invalidate();
                //}
            }
        }

        private void canvas_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // 设置IsTapStop=true之后点击自动获取角度，不需要再设置
            //if (!canvas.Focus(FocusState.Keyboard))
            //    Debug.WriteLine("Cannot set focus to canvas, keyboard not listening");
            //coordText.Visibility = Visibility.Visible;
            if (magnifier != null)
            {
                magnifier.IsVisible = true;
                canvas.Invalidate();
            }
        }

        private void canvas_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //coordText.Visibility = Visibility.Collapsed;
            if (magnifier != null)
            {
                magnifier.IsVisible = false;
                canvas.Invalidate();
            }
        }

        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!canvas.CapturePointer(e.Pointer))
                Debug.WriteLine("Canvas capture pointer failed.");
            watch.Start();

            this.UpdateMagnifier(e.GetCurrentPoint(canvas).Position);
        }

        private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            updatePointerMoved(e.GetCurrentPoint(canvas).Position,
                (e.KeyModifiers & VirtualKeyModifiers.Shift) == VirtualKeyModifiers.Shift,
                (e.KeyModifiers & VirtualKeyModifiers.Control) == VirtualKeyModifiers.Control);
        }

        private void updatePointerMoved(Point point, bool isShiftDown, bool isCtrlDown)
        {
            var x = (float)Math.Ceiling(point.X);
            var y = (float)Math.Ceiling(point.Y);

            if (temp != null)
                temp.UpdatePoint(new Vector2(x, y), isShiftDown, isCtrlDown);

            this.UpdateMagnifier(point);
            this.canvas.Invalidate();
        }

        private void canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (!watch.IsRunning || watch.ElapsedMilliseconds > 300)
                    return;

                var point = e.GetCurrentPoint(canvas).Position;
                var x = (float)point.X;
                var y = (float)point.Y;
                bool isShiftDown = (e.KeyModifiers & VirtualKeyModifiers.Shift) == VirtualKeyModifiers.Shift;
                bool isCtrlDown = (e.KeyModifiers & VirtualKeyModifiers.Control) == VirtualKeyModifiers.Control;
                switch (ActiveTool)
                {
                    case MeasureTool.Pointer:
                        Select(point);
                        break;
                    case MeasureTool.Segment:
                        AddMeasurePoint<MeasureSegment>(x, y, isShiftDown, isCtrlDown);
                        break;
                    case MeasureTool.Angle:
                        AddMeasurePoint<MeasureAngle>(x, y, isShiftDown, isCtrlDown);
                        break;
                    case MeasureTool.Edge:
                        AddMeasurePoint<MeasureRect>(x, y, isShiftDown, isCtrlDown);
                        break;
                    case MeasureTool.Eyedropper:
                        AddMeasurePoint<MeasureColor>(x, y, isShiftDown, isCtrlDown);
                        break;
                    default:
                        break;
                }

                this.UpdateMagnifier(point);
            }
            finally
            {
                canvas.ReleasePointerCapture(e.Pointer);
                watch.Stop();
                watch.Reset();
                this.canvas.Invalidate();
            }
        }
        #endregion

        #region keyboard handler
        private void canvas_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var coreWindow = Window.Current.CoreWindow;
            var position = coreWindow.PointerPosition;

            e.Handled = true;
            switch (e.Key)
            {
                case VirtualKey.Up:
                    position.Y -= 1; break;
                case VirtualKey.Down:
                    position.Y += 1; break;
                case VirtualKey.Left:
                    position.X -= 1; break;
                case VirtualKey.Right:
                    position.X += 1; break;
                default:
                    e.Handled = false;
                    return;
            }
            coreWindow.PointerPosition = position;

            var bounds = Window.Current.Bounds;
            var relative = canvas.TransformToVisual(Window.Current.Content);
            var pointToWindow = relative.TransformPoint(new Point(0, 0));
            var point = new Point(position.X - bounds.Left - pointToWindow.X,
                position.Y - bounds.Top - pointToWindow.Y);
            updatePointerMoved(point, false, false);

        }
        #endregion

        private int undoIndex = -1;
        private void AddMeasurePoint<T>(float x, float y, bool shiftDown, bool isCtrlDown) where T : MeasureObject, new()
        {
            var point = new Vector2((float)Math.Ceiling(x), (float)Math.Ceiling(y));

            for (int i = measureObjects.Count - 1; i >= undoIndex + 1; i--)
                measureObjects.RemoveAt(i);

            try
            {
                if (temp is T)
                {
                    var segement = (T)temp;
                    segement.Pen = Pens[ActiveTool];
                    if (segement.AddPoint(point, shiftDown, isCtrlDown))
                    {
                        measureObjects.Add(temp);
                        SelectedItem = temp;

                        temp = null;
                    }
                }
                else
                {
                    if (temp != null)
                    {
                        temp.Dispose();
                        temp = null;
                    }

                    temp = new T { Context = this, Pen = Pens[ActiveTool] };
                    if (temp.AddPoint(point, shiftDown, isCtrlDown))
                    {
                        measureObjects.Add(temp);
                        SelectedItem = temp;
                        temp = null;
                    }
                }
            }
            finally
            {
                undoIndex = measureObjects.Count - 1;
                CanUndo = true;
                CanRedo = false;
            }
        }

        #region operations
        public void Undo()
        {
            if (!CanUndo) return;
            try
            {
                if (temp != null)
                {
                    if (temp.CanUndo)
                        temp.Undo();
                    else
                    {
                        temp.Dispose();
                        temp = null;
                    }
                    return;
                }
                undoIndex--;
            }
            finally
            {
                updateUndoStates();
            }
        }
        public void Redo()
        {
            if (!CanRedo) return;
            try
            {
                if (temp != null && temp.CanRedo)
                {
                    temp.Redo();
                    return;
                }
                undoIndex++;
            }
            finally
            {
                updateUndoStates();
            }
        }

        private void updateUndoStates()
        {
            canvas.Invalidate();
            CanUndo = temp != null || temp?.CanUndo == true
                || (undoIndex > -1 && undoIndex < measureObjects.Count);
            CanRedo = temp?.CanRedo == true || (measureObjects.Count > 0 && undoIndex < measureObjects.Count - 1);
        }

        public void Select(Point p)
        {
            foreach (var item in measureObjects)
            {
                if (item.HitTest(p))
                {
                    Select(item);
                    return;
                }
            }
            Select(null);
        }

        public void Select(MeasureObject item) => SelectedItem = item;

        public void Clear()
        {
            try
            {
                SelectedItem = null;
                for (int i = measureObjects.Count - 1; i >= 0; i--)
                    measureObjects[i].Dispose();
                measureObjects.Clear();
                temp?.Dispose();
                temp = null;
                undoIndex = -1;
            }
            finally
            {
                updateUndoStates();
            }
        }

        public void Delete()
        {
            if (SelectedItem == null) return;
            try
            {
                if (SelectedItem == temp)
                {
                    temp.Dispose();
                    temp = null;
                    return;
                }

                var index = measureObjects.IndexOf(SelectedItem);

                if (undoIndex + 1 >= measureObjects.Count)
                {
                    measureObjects.RemoveAt(index);
                    measureObjects.Add(SelectedItem);
                }
                else
                {
                    measureObjects.Insert(undoIndex + 1, SelectedItem);
                    measureObjects.RemoveAt(index);
                }

                undoIndex--;
                CanRedo = true;
                CanUndo = undoIndex > -1;
            }
            finally
            {
                SelectedItem = null;
            }
        }

        #endregion
    }
}
