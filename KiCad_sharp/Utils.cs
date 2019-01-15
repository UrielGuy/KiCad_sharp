using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCad
{
    public static class Utils
    {
        public static PointF PointOnCircle(PointF center, double angle, double distance)
        {
            double radians = angle / 180 * Math.PI;
            return new PointF(
                (float)(center.X + distance * Math.Cos(radians)),
                (float)(center.Y - distance * Math.Sin(radians))
                );
        }

        public static double AngleOf(PointF center, PointF point)
        {
            return - Math.Atan2(point.Y - center.Y, point.X - center.X) / Math.PI * 180;
        }

        public static PointF Center(this RectangleF rect)
        {
            return new PointF((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
        }

        public static double Distance(PointF p1, PointF p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}
