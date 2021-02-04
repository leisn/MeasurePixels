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
    public class MeasureAngle : MeasureObject
    {
        public MeasureAngle()
        {
        }

        public float X1 { get; set; } = -1;
        public float X2 { get; set; } = -1;
        public float X3 { get; set; } = -1;
        public float Y1 { get; set; } = -1;
        public float Y2 { get; set; } = -1;
        public float Y3 { get; set; } = -1;

        public Vector2 P1 => new Vector2(X1, Y1);
        public Vector2 P2 => new Vector2(X2, Y2);
        public Vector2 P3 => new Vector2(X3, Y3);

        #region returns
        public double Distance12 => Vector2.Distance(P1, P2);
        public double Distance23 => Vector2.Distance(P2, P3);
        public double Distance13 => Vector2.Distance(P1, P3);
        public double Angle => Angle123;
        public double Angle123 => Radians123 * 180 / Math.PI;
        public double Angle132 => Radians132 * 180 / Math.PI;
        public double Angle213 => Radians213 * 180 / Math.PI;
        public double Radians => Radians123;
        public double Radians123
        {
            get
            {
                var a = Distance23;
                var b = Distance13;
                var c = Distance12;
                var br = (Math.Pow(a, 2) + Math.Pow(c, 2) - Math.Pow(b, 2)) / (2 * a * c);
                return Math.Acos(br);
            }
        }
        public double Radians132
        {
            get
            {
                var a = Distance23;
                var b = Distance13;
                var c = Distance12;
                var cr = (Math.Pow(b, 2) + Math.Pow(a, 2) - Math.Pow(c, 2)) / (2 * a * b);
                return Math.Acos(cr);
            }
        }
        public double Radians213
        {
            get
            {
                var a = Distance23;
                var b = Distance13;
                var c = Distance12;
                var ar = (Math.Pow(b, 2) + Math.Pow(c, 2) - Math.Pow(a, 2)) / (2 * b * c);
                return Math.Acos(ar);
            }
        }
        public double Area => Distance12 * Distance13 * Math.Sin(Radians213) / 2;
        #endregion

        private int currentPoint = 0;
        protected override bool AddPoint(float x, float y, bool shiftDowm, bool _)
        {
            if (currentPoint == 0)
            {
                X3 = X2 = X1 = x;
                Y3 = Y2 = Y1 = y;
                currentPoint++;
                return false;
            }

            UpdatePoint(x, y, shiftDowm, _);

            if (currentPoint < 2)
            {
                currentPoint++;
                return false;
            }
            return true;
        }

        public override bool CanUndo => currentPoint > 1;

        public override void Undo()
        {
            if (!CanUndo) return;
            switch (currentPoint)
            {
                case 2:
                    X3 = X2 = X1;
                    Y3 = Y2 = Y1;
                    break;
            }
            currentPoint--;
        }

        protected override void UpdatePoint(float x, float y, bool shiftDowm, bool _)
        {
            if (currentPoint == 1)
            {
                if (shiftDowm)
                {
                    var hoffset = Math.Abs(x - X1);
                    var voffset = Math.Abs(y - Y1);
                    if (hoffset > voffset)
                        y = Y1;
                    else
                        x = X1;
                }
                X3 = X2 = x;
                Y3 = Y2 = y;
            }
            else
            {
                if (shiftDowm)
                {
                    var hoffset = Math.Abs(x - X2);
                    var voffset = Math.Abs(y - Y2);
                    if (hoffset > voffset)
                        y = Y2;
                    else
                        x = X2;
                }
                X3 = x;
                Y3 = y;
            }
        }


        private bool isP1Upper
        {
            get
            {
                var lineY = (X1 - X2) * (Y3 - Y2) / (X3 - X2) + Y2;
                return Y1 - lineY < 0;
            }
        }

        private int P3Quadrant
        {
            get
            {
                if (X3 < X2)
                    return Y3 > Y2 ? 3 : 2;
                else
                    return Y3 > Y2 ? 4 : 1;
            }
        }

        [Obsolete]
        public void DrawCommon(CanvasDrawingSession g)
        {
            //save tansform
            var save = g.Transform;

            g.FillRectangle(new Rect(X2 - 2, Y2 - 2, 4, 4), Pen.Color);

            using (var pathBuilder = new CanvasPathBuilder(g))
            {
                pathBuilder.BeginFigure(X1, Y1);
                pathBuilder.AddLine(X2, Y2);
                pathBuilder.AddLine(X3, Y3);
                pathBuilder.EndFigure(CanvasFigureLoop.Open);
                g.DrawGeometry(CanvasGeometry.CreatePath(pathBuilder), Pen.Color, Pen.Width);
            }

            #region glyph
            if (IsGlyphVisible)
            {
                var r = (float)Math.Abs(Math.Atan((Y3 - Y2) / (X3 - X2)));
                var s = (float)Radians123;
                var gw = (float)Math.Cos(r) * Pen.GlyphSize;
                var gh = (float)Math.Sin(r) * Pen.GlyphSize;

                switch (P3Quadrant)
                {
                    case 1:
                        r = -r;
                        gh = -gh;
                        break;
                    case 2:
                        gh = -gh;
                        r -= (float)Math.PI;
                        break;
                    case 3:
                        r = (float)Math.PI - r;
                        break;
                    case 4:
                        break;
                }
                switch (P3Quadrant)
                {
                    case 2:
                    case 3:
                        gw = -gw;
                        if (!isP1Upper)
                            s = -s;
                        break;
                    case 1:
                    case 4:
                        if (isP1Upper)
                            s = -s;
                        break;
                }

                using (var pathBuilder = new CanvasPathBuilder(g))
                {
                    pathBuilder.BeginFigure(gw + X2, gh + Y2);
                    pathBuilder.AddArc(new Vector2(X2, Y2), Pen.GlyphSize, Pen.GlyphSize, r, s);
                    pathBuilder.EndFigure(CanvasFigureLoop.Open);
                    g.DrawGeometry(CanvasGeometry.CreatePath(pathBuilder), Pen.Color, Pen.Width);
                }
            }
            #endregion

            //TODO 调整文字显示位置，得到更好的视图
            var radians = -(float)Math.Atan((Y2 - Y3) / (X3 - X2));
            var textLayout = CreateTextLayout(Angle.Round3().ToString() + "°", g);
            var bounds = textLayout.LayoutBounds;
            var size = new Size(bounds.Width + 10, bounds.Height);

            var centerX = X2 + (X3 - X2 - size.Width) / 2;
            var centerY = Y2 + (Y3 - Y2 - size.Height) / 2;
            var ySegement = size.Height / 2 + Pen.Width / 2 + 5;
            var offsetX = Math.Sin(radians) * ySegement;
            var offsetY = Math.Cos(radians) * ySegement;

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
            var geo = CanvasGeometry.CreateRectangle(Context.Creator, new Rect(X2 - 2, Y2 - 2, 4, 4));
            AddGeometry(new GeometryConfig { Geometry = geo, FillColor = Pen.Color });

            using (var pathBuilder = new CanvasPathBuilder(Context.Creator))
            {
                pathBuilder.BeginFigure(X1, Y1);
                pathBuilder.AddLine(X2, Y2);
                pathBuilder.AddLine(X3, Y3);
                pathBuilder.EndFigure(CanvasFigureLoop.Open);
                geo = CanvasGeometry.CreatePath(pathBuilder);
                AddGeometry(new GeometryConfig
                {
                    Geometry = geo,
                    Style=GeometryStyle.Stroke,
                    StrokeColor=Pen.Color,
                    StrokeWidth=Pen.Width
                });
            }

            #region glyph
            if (IsGlyphVisible)
            {
                var r = (float)Math.Abs(Math.Atan((Y3 - Y2) / (X3 - X2)));
                var s = (float)Radians123;
                var gw = (float)Math.Cos(r) * Pen.GlyphSize;
                var gh = (float)Math.Sin(r) * Pen.GlyphSize;

                switch (P3Quadrant)
                {
                    case 1:
                        r = -r;
                        gh = -gh;
                        break;
                    case 2:
                        gh = -gh;
                        r -= (float)Math.PI;
                        break;
                    case 3:
                        r = (float)Math.PI - r;
                        break;
                    case 4:
                        break;
                }
                switch (P3Quadrant)
                {
                    case 2:
                    case 3:
                        gw = -gw;
                        if (!isP1Upper)
                            s = -s;
                        break;
                    case 1:
                    case 4:
                        if (isP1Upper)
                            s = -s;
                        break;
                }

                using (var pathBuilder = new CanvasPathBuilder(Context.Creator))
                {
                    pathBuilder.BeginFigure(gw + X2, gh + Y2);
                    pathBuilder.AddArc(new Vector2(X2, Y2), Pen.GlyphSize, Pen.GlyphSize, r, s);
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
            }
            #endregion

            #region Text
            var radians = -(float)Math.Atan((Y2 - Y3) / (X3 - X2));
            var textLayout = CreateTextLayout(Angle.Round3().ToString() + "°",Context.Creator);
            var bounds = textLayout.LayoutBounds;
            var size = new Size(bounds.Width + 10, bounds.Height);

            var centerX = X2 + (X3 - X2 - size.Width) / 2;
            var centerY = Y2 + (Y3 - Y2 - size.Height) / 2;
            var ySegement = size.Height / 2 + Pen.Width / 2 + 5;
            var offsetX = Math.Sin(radians) * ySegement;
            var offsetY = Math.Cos(radians) * ySegement;

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
