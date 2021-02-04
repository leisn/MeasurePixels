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
    public class MeasureColor : MeasureObject
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Color[] Colors;

        private float radians = 0;

        protected override bool AddPoint(float x, float y, bool shiftDowm, bool ctrlDown)
        {
            if (Colors == null)
            {
                X = x;
                Y = y;
                Colors = Context?.SourceBitmap.GetPixelColors(
                         (int)(X - 1), (int)(Y - 1), 3, 3);
                return false;
            }
            UpdatePoint(x, y, shiftDowm, ctrlDown);
            return true;
        }
        protected override void UpdatePoint(float x, float y, bool shiftDowm, bool ctrlDown)
        {
            int quadrant;
            if (x < X)
                quadrant = y > Y ? 3 : 2;
            else
                quadrant = y > Y ? 4 : 1;

            if (shiftDowm)
            {
                switch (quadrant)
                {
                    case 1:
                        radians = -(float)Math.PI / 2;
                        break;
                    case 2:
                        radians = -(float)Math.PI;
                        break;
                    case 3:
                        radians = -(float)Math.PI / 2 * 3;
                        break;
                    case 4:
                        radians = 0;
                        break;
                }
            }
            else
            {
                if (y - Y != 0 && x - X != 0)
                {
                    radians = (float)Math.Atan((y - Y) / (x - X))
                        - (float)Math.PI / 2; //从正下方开始算
                    if (quadrant == 2)
                        radians -= (float)Math.PI;
                    else if (quadrant == 3)
                        radians += (float)Math.PI;
                }
            }
        }

        [Obsolete]
        public void DrawCommon(CanvasDrawingSession g)
        {
            //save tansform
            var save = g.Transform;

            g.FillRectangle(new Rect(X, Y, Pen.Width, Pen.Width), Pen.Color);

            var c = Colors?[4];
            if (c != null)
            {
                g.Transform = Matrix3x2.CreateRotation(radians, new Vector2(X, Y));

                var color = c.GetValueOrDefault();
                var text = color.ToHex();
                var textLayout = CreateTextLayout(text, g);
                var bounds = textLayout.LayoutBounds;
                var size = new Size(bounds.Width + 10, bounds.Height);

                var colorBlock = new Rect(X + Pen.Width / 2, Y + Pen.Width * 3 / 2 + 5,
                    Pen.GlyphSize, Pen.GlyphSize);
                if (IsGlyphVisible)
                {
                    g.FillRectangle(colorBlock, color);
                    g.DrawRectangle(colorBlock, Pen.Color, Pen.Width);
                    colorBlock.X -= Pen.Width / 2;
                    colorBlock.Y += colorBlock.Height + Pen.Width / 2 + 5;
                }

                var left = colorBlock.Left;
                var top = colorBlock.Top;

                var rect = new Rect(left, top, size.Width, size.Height);
                g.FillRectangle(rect, Pen.TextBackground);

                g.DrawTextLayout(textLayout, (float)left + 5, (float)top, Pen.TextColor);
            }

            //recover transform
            g.Transform = save;
        }

        protected override void UpdateGeometries()
        {
            var geo = CanvasGeometry.CreateRectangle(Context.Creator, new Rect(X, Y, Pen.Width, Pen.Width));
            AddGeometry(new GeometryConfig { Geometry = geo, FillColor = Pen.Color });

            var c = Colors?[4];
            if (c == null) return;
            var color = c.GetValueOrDefault();
            var colorBlock = new Rect(X + Pen.Width / 2, Y + Pen.Width * 3 / 2 + 5,
                    Pen.GlyphSize, Pen.GlyphSize);

            var transform = Matrix3x2.CreateRotation(radians, new Vector2(X, Y));
            if (IsGlyphVisible)
            {
                geo = CanvasGeometry.CreateRectangle(Context.Creator, colorBlock)
                    .Transform(transform);
                colorBlock.X -= Pen.Width / 2;
                colorBlock.Y += colorBlock.Height + Pen.Width / 2 + 5;
                AddGeometry(new GeometryConfig
                {
                    Geometry = geo,
                    Style = GeometryStyle.Both,
                    FillColor = color,
                    StrokeColor = Pen.Color,
                    StrokeWidth = Pen.Width
                });
            }

            #region text
            var textLayout = CreateTextLayout(color.ToHex(), Context.Creator);
            var bounds = textLayout.LayoutBounds;
            var size = new Size(bounds.Width + 10, bounds.Height);

            var left = colorBlock.Left;
            var top = colorBlock.Top;

            geo = CanvasGeometry.CreateRectangle(Context.Creator,
               new Rect(left, top, size.Width, size.Height)).Transform(transform);

            AddGeometry(new GeometryConfig
            {
                Geometry = geo,
                FillColor = Pen.TextBackground
            });

            geo = CanvasGeometry.CreateText(textLayout).Transform(
                Matrix3x2.CreateTranslation((float)left + 5, (float)top)).Transform(transform);

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
