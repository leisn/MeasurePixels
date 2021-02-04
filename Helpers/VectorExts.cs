using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Windows.Foundation;

namespace MeasurePixels
{
    public static class VectorExts
    {
        public static bool IsUpper(this Vector2 P, Vector2 P1, Vector2 P2)
        {
            var lineY = (P.X - P1.X) * (P2.Y - P1.Y) / (P2.X - P1.X) + P1.Y;
            return P.Y - lineY < 0;
        }

        public static float Distance(this Vector2 P, Vector2 P1, Vector2 P2)
        {
            if (P1.X == P2.X)
                return Math.Abs(P.X - P1.X);
            var k = (P2.Y - P1.Y) / (P2.X - P1.X);
            var c = (P2.X * P1.Y - P1.X * P2.Y) / (P2.X - P1.X);
            return (float)Math.Abs((k * P.X - P.Y + c) / (Math.Sqrt(k * k + 1)));
        }

        /// <summary>
        /// 返回 点 Normaliized后所在的象限<br/>
        /// 2 | 1 <br/>
        /// ----- <br/>
        /// 3 | 4 <br/>
        /// </summary>
        public static int Quadrant(this Vector2 point)
        {
            var normal = Vector2.Normalize(point);
            if (normal.X < 0)
                return normal.Y > 0 ? 3 : 2;
            else
                return normal.X > 0 ? 4 : 1;
        }

        public static int Quadrant(this Vector2 self, Vector2 point)
        {
            if (point.X < self.X)
                return point.Y > self.Y ? 3 : 2;
            else
                return point.Y > self.Y ? 4 : 1;
        }


        /// <summary>
        /// 自身与另一个点组成的线上,距离自身distance的点
        /// </summary>
        public static Vector2 GetLinePoint(this Vector2 self, Vector2 end, float distance)
        {
            var offset = Vector2.Normalize(end - self) * distance;
            return new Vector2(self.X + offset.X, self.Y + offset.Y);
        }

        public static Point GetLinePoint(this Point self, Point end, float distance)
        {
            var offset = Vector2.Normalize(end.ToVector2() - self.ToVector2()) * distance;
            return new Point(self.X + offset.X, self.Y + offset.Y);
        }

        /// <summary>
        /// 自身与另一个点组成的线上,x轴坐标为x时 y的值
        /// </summary>
        public static float GetLinePointX(this Vector2 self, Vector2 linePoint, float x)
        {
            return (x - self.X) * (linePoint.Y - self.Y) / (linePoint.X - self.X) + self.Y;
        }
        /// <summary>
        /// 自身与另一个点组成的线上,y轴坐标为y时 x的值
        /// </summary>
        public static float GetLinePointY(this Vector2 self, Vector2 linePoint, float y)
        {
            return (y - self.Y) * (linePoint.X - self.X) / (linePoint.Y - self.Y) + self.X;
        }

        /// <summary>
        /// 自身为中心点的弧度
        /// </summary>
        public static float Radian(this Vector2 P, Vector2 P1, Vector2 P2)
        {
            var a = Vector2.Distance(P2, P);
            var b = Vector2.Distance(P1, P);
            var c = Vector2.Distance(P1, P2);
            var cr = (Math.Pow(b, 2) + Math.Pow(a, 2) - Math.Pow(c, 2)) / (2 * a * b);
            return (float)Math.Acos(cr);
        }
        /// <summary>
        /// 自身为中心点的角度
        /// </summary>
        public static float Angle(this Vector2 P, Vector2 P1, Vector2 P2)
            => P.Radian(P1, P2).ToAngle();

        public static Point ToPoint(this Vector2 self) => new Point(self.X, self.Y);
        public static Vector2 ToVector2(this Point self) => new Vector2((float)self.X, (float)self.Y);

        /// <summary>
        /// 自身与另一个点组成的线与X轴的弧度
        /// </summary>
        public static float RadianX(this Vector2 self, Vector2 point)
            => (float)Math.Atan((self.Y - point.Y) / (point.X - self.X));

        public static double ToRadian(this double angle) => angle * Math.PI / 180;
        public static double ToAngle(this double radian) => radian * 180 / Math.PI;
        public static float ToRadian(this float angle) => (float)(angle * Math.PI / 180);
        public static float ToAngle(this float radian) => (float)(radian * 180 / Math.PI);

        public static float Round3(this double self) => (float)Math.Round(self, 3);
        public static float Round3(this float self) => (float)Math.Round(self, 3);
    }
}
