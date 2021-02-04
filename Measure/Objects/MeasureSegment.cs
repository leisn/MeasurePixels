using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class MeasureSegment : MeasureObject
    {
        public float X1 { get; set; } = -1;
        public float X2 { get; set; } = -1;
        public float Y1 { get; set; } = -1;
        public float Y2 { get; set; } = -1;

        public Vector2 P1 => new Vector2(X1, Y1);
        public Vector2 P2 => new Vector2(X2, Y2);

        public double Distance => Vector2.Distance(P1, P2);
        public double Angle => Radians * 180 / Math.PI;
        public double Radians => Math.Atan((Y1 - Y2) / (X2 - X1));

        protected override bool AddPoint(float x, float y, bool shiftDowm, bool _)
        {
            x = (float)Math.Ceiling(x);
            y = (float)Math.Ceiling(y);
            if (X1 == -1 || Y1 == -1)
            {
                X2 = X1 = x;
                Y2 = Y1 = y;
                return false;
            }
            UpdatePoint(x, y, shiftDowm, _);
            return true;
        }

        protected override void UpdatePoint(float x, float y, bool shiftDowm, bool _)
        {
            x = (float)Math.Ceiling(x);
            y = (float)Math.Ceiling(y);
            if (shiftDowm)
            {
                var hoffset = Math.Abs(x - X1);
                var voffset = Math.Abs(y - Y1);
                if (hoffset > voffset)
                    y = Y1;
                else
                    x = X1;
            }
            X2 = x;
            Y2 = y;
        }

        [Obsolete]
        public void DrawCommon(CanvasDrawingSession g)
        {

            if (X1 == -1 || Y1 == -1)
                return;
            if (X1 == X2 && Y1 == Y2)
                return;

            g.DrawLine(X1, Y1, X2, Y2, Pen.Color, Pen.Width);

            //save tansform
            var save = g.Transform;

            var radians = -(float)Radians;//反方向旋转绘图

            if (IsGlyphVisible)
            {
                g.Transform = Matrix3x2.CreateRotation(radians, new Vector2(X1, Y1));
                g.DrawLine(X1, Y1 - Pen.GlyphSize / 2, X1, Y1 + Pen.GlyphSize / 2,
                    Pen.Color, Pen.Width);

                g.Transform = Matrix3x2.CreateRotation(radians, new Vector2(X2, Y2));
                g.DrawLine(X2, Y2 - Pen.GlyphSize / 2, X2, Y2 + Pen.GlyphSize / 2,
                  Pen.Color, Pen.Width);

            }

            var textLayout = CreateTextLayout(Distance.Round3().ToString(), g);

            var bounds = textLayout.LayoutBounds;
            var size = new Size(bounds.Width + 10, bounds.Height);

            var centerX = X1 + (X2 - X1 - size.Width) / 2;
            var centerY = Y1 + (Y2 - Y1 - size.Height) / 2;
            var ySegment = size.Height / 2 + Pen.Width / 2 + 5;
            var offsetX = Math.Sin(radians) * ySegment;
            var offsetY = Math.Cos(radians) * ySegment;

            offsetX = Math.Abs(offsetX);
            offsetX = radians < 0 ? offsetX : -offsetX;

            var left = centerX - offsetX;
            var top = centerY - offsetY;
            centerX = left + size.Width / 2;
            centerY = top + size.Height / 2;

            g.Transform = Matrix3x2.CreateRotation(radians, new Vector2((float)centerX, (float)centerY));
            var rect = new Rect(left, top, size.Width, size.Height);
            g.FillRectangle(rect, Pen.TextBackground);
            g.DrawTextLayout(textLayout, (float)left + 5, (float)top, Pen.TextColor);

            //recover transform
            g.Transform = save;

        }

        protected override void UpdateGeometries()
        {

            var geo = CanvasGeometry.CreateRectangle(Context.Creator,
               new Rect(X1- 2, Y1 - 2, 4, 4));
            AddGeometry(new GeometryConfig { Geometry = geo, FillColor = Pen.Color });

            var radians = -(float)Radians;

            using (var pathBuilder = new CanvasPathBuilder(Context.Creator))
            {
                pathBuilder.BeginFigure(X1, Y1);
                pathBuilder.AddLine(X2, Y2);
                pathBuilder.EndFigure(CanvasFigureLoop.Open);
                geo = CanvasGeometry.CreatePath(pathBuilder);
                AddGeometry(new GeometryConfig
                {
                    Geometry = geo,
                    Style = GeometryStyle.Stroke,
                    StrokeColor = Pen.Color,
                    StrokeWidth = Pen.Width
                });
            }

            if (IsGlyphVisible)
            {
                geo = CanvasGeometry.CreateRectangle(Context.Creator,
                new Rect(X1, Y1 - Pen.GlyphSize / 2, Pen.Width, Pen.GlyphSize))
                    .Transform(Matrix3x2.CreateRotation(radians, new Vector2(X1, Y1)));
                AddGeometry(new GeometryConfig
                {
                    Geometry = geo,
                    FillColor = Pen.Color
                });

                geo = CanvasGeometry.CreateRectangle(Context.Creator,
                    new Rect(X2, Y2 - Pen.GlyphSize / 2, Pen.Width, Pen.GlyphSize))
                   .Transform(Matrix3x2.CreateRotation(radians, new Vector2(X2, Y2)));
                AddGeometry(new GeometryConfig
                {
                    Geometry = geo,
                    FillColor = Pen.Color,
                });
            }
          
            #region Text
            var textLayout = CreateTextLayout(Distance.Round3().ToString(), Context.Creator);

            var bounds = textLayout.LayoutBounds;
            var size = new Size(bounds.Width + 10, bounds.Height);

            var centerX = X1 + (X2 - X1 - size.Width) / 2;
            var centerY = Y1 + (Y2 - Y1 - size.Height) / 2;
            var ySegment = size.Height / 2 + Pen.Width / 2 + 5;
            var offsetX = Math.Sin(radians) * ySegment;
            var offsetY = Math.Cos(radians) * ySegment;

            offsetX = Math.Abs(offsetX);
            offsetX = radians < 0 ? offsetX : -offsetX;

            var left = centerX - offsetX;
            var top = centerY - offsetY;

            var rotation = Matrix3x2.CreateRotation(radians, new Vector2((float)(left + size.Width / 2), (float)(top + size.Height / 2)));
            geo = CanvasGeometry
                .CreateRectangle(Context.Creator, new Rect(left, top, size.Width, size.Height))
                .Transform(rotation);

            AddGeometry(new GeometryConfig
            {
                Geometry = geo,
                FillColor = Pen.TextBackground
            });

            geo = CanvasGeometry
                .CreateText(textLayout)
                .Transform(Matrix3x2.CreateTranslation((float)left + 5, (float)top))
                .Transform(rotation);

            AddGeometry(new GeometryConfig
            {
                Geometry = geo,
                IsHitTestUnit = false,
                FillColor = Pen.TextColor
            });
            #endregion
        }
    }
}
