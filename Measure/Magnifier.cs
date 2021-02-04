using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

using Windows.Foundation;
using Windows.UI;

namespace MeasurePixels.Measure
{
    public enum MagnifierPostion
    {
        Center,
        Outer
    }

    public class Magnifier : IDisposable
    {
        CanvasImageBrush imageBrush;
        public bool IsVisible { get; set; } = false;
        public float CenterX { get; set; }
        public float CenterY { get; set; }
        public float Radius { get; set; }
        public float Scale { get; set; } = 4f;

        public MagnifierPostion Postion { get; set; } = MagnifierPostion.Outer;

        public Magnifier(ICanvasResourceCreator creator)
        {
            imageBrush = new CanvasImageBrush(creator)
            {
                ExtendX = CanvasEdgeBehavior.Clamp,
                ExtendY = CanvasEdgeBehavior.Clamp,
                Interpolation = CanvasImageInterpolation.NearestNeighbor
            };

        }

        public ICanvasImage Source
        {
            get => imageBrush.Image;
            set
            {
                if (imageBrush.Image != value)
                    imageBrush.Image = value;
            }
        }

        public void Draw(CanvasDrawingSession g, Size canvasSize, float startX = 0, float startY = 0)
        {
            if (!IsVisible)
                return;

            if (Postion == MagnifierPostion.Outer)
            {
                var offsetH = Radius + 16;
                var offsetV = Radius + 20;
                if (CenterX + offsetH + Radius > canvasSize.Width)
                    offsetH = -offsetH;
                if (CenterY + offsetV + Radius > canvasSize.Height)
                    offsetV = -offsetV;

                startX -= (CenterX - startX) * (Scale - 1) - offsetH;
                startY -= (CenterY - startY) * (Scale - 1) - offsetV;
                CenterX += offsetH;
                CenterY += offsetV;
            }
            else
            {
                startX -= (CenterX - startX) * (Scale - 1);
                startY -= (CenterY - startY) * (Scale - 1);
            }

            imageBrush.Transform = Matrix3x2.Multiply(
              Matrix3x2.CreateScale(Scale),
                  Matrix3x2.CreateTranslation(startX, startY));

            g.FillCircle(CenterX, CenterY, Radius, imageBrush);

            g.DrawLine(CenterX, CenterY - Radius / 3 * 2, CenterX, CenterY + Radius / 3 * 2,
                Colors.DeepSkyBlue, Scale);
            g.DrawLine(CenterX - Radius / 3 * 2, CenterY, CenterX + Radius / 3 * 2, CenterY,
                Colors.DeepSkyBlue, Scale);

            g.DrawCircle(CenterX, CenterY, Radius, Colors.Black, 3);
        }

        public void Dispose()
        {
            imageBrush?.Dispose();
            imageBrush = null;
        }
    }
}
