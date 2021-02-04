using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

using Windows.Foundation;
using Windows.UI;

namespace MeasurePixels.Measure.Objects
{
    public enum GeometryStyle
    {
        Stroke, Fill, Both
    }

    public class GeometryConfig : IDisposable
    {
        public CanvasGeometry Geometry { get; set; }
        public GeometryStyle Style { get; set; } = GeometryStyle.Fill;
        public Color StrokeColor { get; set; }
        public float StrokeWidth { get; set; }
        public Color FillColor { get; set; }
        public bool IsHitTestUnit { get; set; } = true;
        public bool IsDrawUnit { get; set; } = true;

        public void Dispose()
        {
            Geometry?.Dispose();
            Geometry = null;
        }
    }

    public abstract class MeasureObject : IDisposable
    {
        readonly List<GeometryConfig> Geometries = new List<GeometryConfig>();

        public MeasurePen Pen { get; set; } = MeasurePen.Default;

        public bool IsCompleted => graphicsCompleted;

        public IMeasureCanvas Context { get; set; }

        public event Action Completed;

        private bool graphicsCompleted = false;
        protected bool IsGlyphVisible => Pen.GlyphSize > 0;
        public MeasureObject() { }

        public virtual bool CanUndo => false;
        public virtual void Undo() { }
        public virtual bool CanRedo => false;
        public virtual void Redo() { }

        public bool IsSelected { get; set; }

        public bool HitTest(Point p) => HitTest(new Vector2((float)p.X, (float)p.Y));

        public virtual bool HitTest(Vector2 p)
        {
            foreach (var item in Geometries)
            {
                if (!item.IsHitTestUnit)
                    continue;
                bool hit = false;
                switch (item.Style)
                {
                    case GeometryStyle.Stroke:
                        hit = item.Geometry.StrokeContainsPoint(p, Pen.Width);
                        break;
                    case GeometryStyle.Fill:
                        hit = item.Geometry.FillContainsPoint(p);
                        break;
                    case GeometryStyle.Both:
                        hit = item.Geometry.StrokeContainsPoint(p, Pen.Width) ||
                              item.Geometry.FillContainsPoint(p);
                        break;
                }
                if (hit) return true;
            }
            return false;
        }

        public virtual void Draw(CanvasDrawingSession g, bool drawSelected = true)
        {
            if (drawSelected && IsSelected)
            {
                var groups = new CanvasGeometry[Geometries.Count];
                for (int i = 0; i < groups.Length; i++)
                    groups[i] = Geometries[i].Geometry;
                using (var group = CanvasGeometry.CreateGroup(g, groups))
                {
                    var bounds = group.ComputeBounds();
                    g.FillRectangle(bounds, Color.FromArgb(0x22, 0, 0, 0));
                    g.DrawRectangle(bounds, Context.SelectedBorderColor, Context.SelectedBorderWidth);
                }
            }

            foreach (var item in Geometries)
            {
                if (!item.IsDrawUnit)
                    continue;
                switch (item.Style)
                {
                    case GeometryStyle.Stroke:
                        g.DrawGeometry(item.Geometry, item.StrokeColor, item.StrokeWidth);
                        break;
                    case GeometryStyle.Fill:
                        g.FillGeometry(item.Geometry, item.FillColor);
                        break;
                    case GeometryStyle.Both:
                        g.DrawGeometry(item.Geometry, item.StrokeColor, item.StrokeWidth);
                        g.FillGeometry(item.Geometry, item.FillColor);
                        break;
                }
            }
        }

        //start->addpoint-> updatepoint->addpoint->over
        public bool AddPoint(Vector2 point, bool shiftDowm, bool ctrlDown)
        {
            graphicsCompleted = AddPoint(point.X, point.Y, shiftDowm, ctrlDown);
            this.ClearGeometries();
            this.UpdateGeometries();
            if (graphicsCompleted)
                Completed?.Invoke();
            return graphicsCompleted;
        }

        public void UpdatePoint(Vector2 lastPoint, bool shiftDowm, bool ctrlDown)
        {
            if (graphicsCompleted)
                return;
            UpdatePoint(lastPoint.X, lastPoint.Y, shiftDowm, ctrlDown);
            this.ClearGeometries();
            this.UpdateGeometries();
        }

        protected abstract void UpdateGeometries();
        protected abstract bool AddPoint(float x, float y, bool shiftDowm, bool ctrlDown);
        protected abstract void UpdatePoint(float x, float y, bool shiftDowm, bool ctrlDown);

        public virtual CanvasTextLayout CreateTextLayout(string text, ICanvasResourceCreator g)
        {
            return new CanvasTextLayout(g, text, new CanvasTextFormat
            {
                FontFamily = Pen.FontFamily,
                FontSize = Pen.FontSize,
                WordWrapping = CanvasWordWrapping.NoWrap
            }, 0, 0);
        }

        public Point ClipStarts(double x, double y) => new Point(x - Context.StartX, y - Context.StartY);

        public virtual void Dispose()
        {
            Context = null;
            ClearGeometries();
        }

        private void ClearGeometries()
        {
            for (int i = Geometries.Count - 1; i >= 0; i--)
                Geometries[i].Dispose();
            Geometries.Clear();
        }

        protected void AddGeometry(GeometryConfig conf) => Geometries.Add(conf);
    }
}
