using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

using Windows.Foundation;
using Windows.UI;

namespace MeasurePixels.Measure.Objects
{
    public class MeasureRect : MeasureObject
    {
        public Vector2 P11 { get; set; }
        public Vector2 P12 { get; set; }
        public Vector2 P21 { get; set; }
        public Vector2 P22 { get; set; }


        #region values
        public double Distance12 => Vector2.Distance(P11, P12);
        public double Distance23 => Vector2.Distance(P12, P22);
        public double Distance13 => Vector2.Distance(P11, P22);
        public double Distance34 => Vector2.Distance(P22, P21);
        public double Distance14 => Vector2.Distance(P11, P21);
        public double Distance24 => Vector2.Distance(P12, P21);
        public double Angle1 => P11.Angle(P12, P21);
        public double Angle2 => P12.Angle(P11, P22);
        public double Angle3 => P22.Angle(P12, P21);
        public double Angle4 => P21.Angle(P11, P22);
        public double Angle124 => P12.Angle(P11, P21);
        public double Angle231 => P22.Angle(P11, P12);
        public double Angle342 => P21.Angle(P22, P12);
        public double Angle413 => P11.Angle(P21, P22);
        #endregion

        private int currentIndex = 0;

        protected override bool AddPoint(float x, float y, bool shiftDowm, bool ctrlDown)
        {
            currentIndex++;
            if (currentIndex == 1)
            {
                P22 = P21 = P12 = P11 = new Vector2(x, y);
                return false;
            }

            UpdatePoint(x, y, shiftDowm, ctrlDown);
            if (ctrlDown)
                currentIndex = 4;
            return currentIndex > 3;
        }

        protected override void UpdatePoint(float x, float y, bool shiftDowm, bool ctrlDown)
        {
            //ctrl 保持平行，ctrl+shift 保持直角 闭合, shift最后闭合
            if (currentIndex == 1)
            {
                if (ctrlDown)
                {
                    P12 = new Vector2(x, P11.Y);
                    if (shiftDowm)//正方形
                    {
                        var width = x - P11.X;
                        P21 = new Vector2(P11.X, P11.Y + width);
                        P22 = new Vector2(x, P11.Y + width);
                    }
                    else//长方形
                    {
                        P21 = new Vector2(P11.X, y);
                        P22 = new Vector2(x, y);
                    }
                }
                else
                {
                    if (shiftDowm)
                    {
                        var hoffset = Math.Abs(x - P11.X);
                        var voffset = Math.Abs(y - P11.Y);
                        if (hoffset > voffset)
                            y = P11.Y;
                        else
                            x = P11.X;
                    }
                    P22 = P21 = P12 = new Vector2(x, y);
                }

            }
            else if (currentIndex == 2)
            {
                if (ctrlDown)
                {
                    var point = new Vector2(x, y);
                    var l12 = Vector2.Distance(P11, P12);
                    var radians = Math.Atan((P11.Y - P12.Y) / (P12.X - P11.X));
                    var oy = y - P12.Y;
                    var ox = (float)(oy * Math.Tan(radians));
                    if (shiftDowm)
                    {
                        ox = (float)(l12 * Math.Sin(radians));
                        oy = (float)(l12 * Math.Cos(radians));
                        if (point.IsUpper(P11, P12))
                        {
                            ox = -ox;
                            oy = -oy;
                        }
                    }
                    P22 = new Vector2(P12.X + ox, P12.Y + oy);
                    P21 = new Vector2(P11.X + ox, P11.Y + oy);
                }
                else
                {
                    if (shiftDowm)
                    {
                        var hoffset = Math.Abs(x - P12.X);
                        var voffset = Math.Abs(y - P12.Y);
                        if (hoffset > voffset)
                            y = P12.Y;
                        else
                            x = P12.X;
                    }
                    P22 = P21 = new Vector2(x, y);
                }
            }
            else
            {
                if (ctrlDown)
                {
                    if (shiftDowm)
                    {
                        var d = P22.Distance(P11, P12);
                        var rx = P11.RadianX(P12);

                        var ox = (float)Math.Sin(rx) * d;
                        var oy = (float)Math.Cos(rx) * d;
                        var temp = new Vector2(P11.X + ox, P11.Y + oy);
                        if (temp.IsUpper(P11, P12) != P22.IsUpper(P11, P12))
                        {
                            ox = -ox;
                            oy = -oy;
                        }

                        P21 = new Vector2(P11.X + ox, P11.Y + oy);
                    }
                    else
                        P21 = new Vector2(P11.X + P22.X - P12.X, P11.Y + P22.Y - P12.Y);
                }
                else
                {
                    if (shiftDowm)
                    {
                        var hoffset = Math.Abs(x - P22.X);
                        var voffset = Math.Abs(y - P22.Y);
                        if (hoffset > voffset)
                            y = P22.Y;
                        else
                            x = P22.X;
                    }
                    P21 = new Vector2(x, y);
                }
            }

        }

        public override bool CanUndo => currentIndex > 1;

        public override void Undo()
        {
            if (!CanUndo) return;

            switch (currentIndex)
            {
                case 2:
                    P21 = P22 = P12 = P11;
                    break;
                case 3:
                    P21 = P22 = P12;
                    break;
                default:
                    break;
            }
            currentIndex--;
        }

        /**[Obsolete]
        public void DrawCommon(CanvasDrawingSession g)
        {
            //save tansform
            var save = g.Transform;

            g.FillRectangle(new Rect(P11.X - 2, P11.Y - 2, 4, 4), Pen.Color);

            using (var pathBuilder = new CanvasPathBuilder(g))
            {
                pathBuilder.BeginFigure(P11);
                pathBuilder.AddLine(P12);
                pathBuilder.AddLine(P22);
                pathBuilder.AddLine(P21);
                pathBuilder.EndFigure(CanvasFigureLoop.Closed);
                g.DrawGeometry(CanvasGeometry.CreatePath(pathBuilder), Pen.Color, Pen.Width);
            }

            if (IsGlyphVisible)
            {
                drawOrthogonalGlyph(g, P11, P12, P21);
                drawOrthogonalGlyph(g, P12, P11, P22);
                drawOrthogonalGlyph(g, P21, P11, P22);
                drawOrthogonalGlyph(g, P22, P21, P12);
            }

            drawMeasure(g, P11, P12);
            drawMeasure(g, P12, P22);
            drawMeasure(g, P22, P21);
            drawMeasure(g, P21, P11);

            //recover transform
            g.Transform = save;
        }
        private void drawOrthogonalGlyph(CanvasDrawingSession g, Vector2 center, Vector2 start, Vector2 end)
        {
            if (Math.Round(center.Angle(start, end), 3) == 90)
            {
                using (var pathBuilder = new CanvasPathBuilder(g))
                {
                    var p1 = center.GetLinePoint(start, Pen.GlyphSize);
                    var p2 = center.GetLinePoint(end, Pen.GlyphSize);
                    var p3 = new Vector2(p1.X + p2.X - center.X, p1.Y + p2.Y - center.Y);
                    pathBuilder.BeginFigure(p1);
                    pathBuilder.AddLine(p3);
                    pathBuilder.AddLine(p2);
                    pathBuilder.EndFigure(CanvasFigureLoop.Open);
                    g.DrawGeometry(CanvasGeometry.CreatePath(pathBuilder), Pen.Color, Pen.Width);
                }
            }
        }
        private void drawMeasure(CanvasDrawingSession g, Vector2 start, Vector2 end)
        {
            var textLayout = CreateTextLayout(Vector2.Distance(start, end).Round3().ToString(), g);

            var bounds = textLayout.LayoutBounds;
            var size = new Size(bounds.Width + 10, bounds.Height);

            var ySegment = size.Height / 2 + Pen.Width / 2 + 5;
            var q = (end - start).Quadrant();
            switch (q)
            {
                case 2:
                case 3:
                    ySegment = -ySegment;
                    break;
                case 1:
                case 4:
                    break;
            }

            var centerX = start.X + (end.X - start.X - size.Width) / 2;
            var centerY = start.Y + (end.Y - start.Y - size.Height) / 2;

            var radians = start.RadianX(end);

            var offsetX = Math.Sin(radians) * ySegment;
            var offsetY = Math.Cos(radians) * ySegment;

            var left = centerX - offsetX;
            var top = centerY - offsetY;
            centerX = left + size.Width / 2;
            centerY = top + size.Height / 2;

            g.Transform = Matrix3x2.CreateRotation(-radians, new Vector2((float)centerX, (float)centerY));

            var rect = new Rect(left, top, size.Width, size.Height);
            g.FillRectangle(rect, Pen.TextBackground);
            g.DrawTextLayout(textLayout, (float)left + 5, (float)top, Pen.TextColor);
        }*/

        protected override void UpdateGeometries()
        {
            var geo = CanvasGeometry.CreateRectangle(Context.Creator,
                new Rect(P11.X - 2, P11.Y - 2, 4, 4));
            AddGeometry(new GeometryConfig { Geometry = geo, FillColor = Pen.Color });

            using (var pathBuilder = new CanvasPathBuilder(Context.Creator))
            {
                pathBuilder.BeginFigure(P11);
                pathBuilder.AddLine(P12);
                pathBuilder.AddLine(P22);
                pathBuilder.AddLine(P21);
                pathBuilder.EndFigure(CanvasFigureLoop.Closed);

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
                updateGlyphGeometries(Context.Creator, P11, P12, P21);
                updateGlyphGeometries(Context.Creator, P12, P11, P22);
                updateGlyphGeometries(Context.Creator, P21, P11, P22);
                updateGlyphGeometries(Context.Creator, P22, P21, P12);
            }

            udpateMeasureGeometries(Context.Creator, P11, P12);
            udpateMeasureGeometries(Context.Creator, P12, P22);
            udpateMeasureGeometries(Context.Creator, P22, P21);
            udpateMeasureGeometries(Context.Creator, P21, P11);
        }
        private void updateGlyphGeometries(ICanvasResourceCreator g, Vector2 center, Vector2 start, Vector2 end)
        {
            if (Math.Round(center.Angle(start, end), 3) == 90)
            {
                using (var pathBuilder = new CanvasPathBuilder(g))
                {
                    var p1 = center.GetLinePoint(start, Pen.GlyphSize);
                    var p2 = center.GetLinePoint(end, Pen.GlyphSize);
                    var p3 = new Vector2(p1.X + p2.X - center.X, p1.Y + p2.Y - center.Y);
                    pathBuilder.BeginFigure(p1);
                    pathBuilder.AddLine(p3);
                    pathBuilder.AddLine(p2);
                    pathBuilder.EndFigure(CanvasFigureLoop.Open);
                    var geo = CanvasGeometry.CreatePath(pathBuilder);
                    AddGeometry(new GeometryConfig
                    {
                        Geometry = geo,
                        Style = GeometryStyle.Stroke,
                        StrokeColor = Pen.Color,
                        StrokeWidth = Pen.Width
                    });
                }
            }
        }
        private void udpateMeasureGeometries(ICanvasResourceCreator g, Vector2 start, Vector2 end)
        {
            var textLayout = CreateTextLayout(Vector2.Distance(start, end).Round3().ToString(), g);

            var bounds = textLayout.LayoutBounds;
            var size = new Size(bounds.Width + 10, bounds.Height);

            var ySegment = size.Height / 2 + Pen.Width / 2 + 5;
            var q = (end - start).Quadrant();
            switch (q)
            {
                case 2:
                case 3:
                    ySegment = -ySegment;
                    break;
                case 1:
                case 4:
                    break;
            }

            var centerX = start.X + (end.X - start.X - size.Width) / 2;
            var centerY = start.Y + (end.Y - start.Y - size.Height) / 2;

            var radians = start.RadianX(end);

            var offsetX = Math.Sin(radians) * ySegment;
            var offsetY = Math.Cos(radians) * ySegment;

            var left = centerX - offsetX;
            var top = centerY - offsetY;

            radians = -radians;

            var rotation = Matrix3x2.CreateRotation(radians, new Vector2((float)(left + size.Width / 2), (float)(top + size.Height / 2)));
            var geo = CanvasGeometry
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
        }
    }
}
