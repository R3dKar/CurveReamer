using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CurveUnfolder
{
    public class PathCubicBezierSegment : PathSegment
    {
        public PointD ControlPoint1 { get; set; }
        public PointD ControlPoint2 { get; set; }

        public PathCubicBezierSegment(PointD startPoint, PointD controlPoint1, PointD controlPoint2, PointD endPoint, bool relative = false)
        {
            StartPoint = startPoint;
            if (relative)
            {
                ControlPoint1 = startPoint + controlPoint1;
                ControlPoint2 = startPoint + controlPoint2;
                EndPoint = startPoint + endPoint;
            }
            else
            {
                ControlPoint1 = controlPoint1;
                ControlPoint2 = controlPoint2;
                EndPoint = endPoint;
            }
        }
        public override PathSegment Clone()
        {
            return new PathCubicBezierSegment(StartPoint, ControlPoint1, ControlPoint2, EndPoint);
        }

        public override double GetLength()
        {
            double length = 0f;

            for (int i = 0; i < Approximated.Length - 1; i++)
            {
                length += LineDistance(Approximated[i], Approximated[i + 1]);
            }

            return length;
        }

        public override double[] GetSize()
        {
            double[] size = new double[4] { StartPoint.X, StartPoint.Y, StartPoint.X, StartPoint.Y };

            foreach (var current in Approximated)
            {
                size[0] = Math.Min(size[0], current.X);
                size[1] = Math.Min(size[1], current.Y);
                size[2] = Math.Max(size[2], current.X);
                size[3] = Math.Max(size[3], current.Y);
            }

            return size;
        }

        private static double LineDistance(PointD start, PointD end)
        {
            return Math.Sqrt((start.X - end.X) * (start.X - end.X) + (start.Y - end.Y) * (start.Y - end.Y));
        }

        private static double Lerp(double a, double b, double t) => a + (b - a) * t;

        private static double GetCubicAtT(double t, double start, double c1, double c2, double end)
        {
            double l1 = Lerp(start, c1, t);
            double l2 = Lerp(c1, c2, t);
            double l3 = Lerp(c2, end, t);
            double a = Lerp(l1, l2, t);
            double b = Lerp(l2, l3, t);
            return Lerp(a, b, t);
        }

        public override void Scale(double scale)
        {
            StartPoint *= scale;
            ControlPoint1 *= scale;
            ControlPoint2 *= scale;
            EndPoint *= scale;
            approximated = null;
        }

        public override void Translate(double x, double y)
        {
            PointD offset = new PointD(x, y);
            StartPoint += offset;
            ControlPoint1 += offset;
            ControlPoint2 += offset;
            EndPoint += offset;
            approximated = null;
        }

        private PointD[] approximated;
        public PointD[] Approximated { get => approximated ?? Approximate(); }
        public override PointD[] Approximate()
        {
            if (approximated != null)
                return (PointD[])approximated.Clone();

            List<PointD> points = new List<PointD>();
            points.Add(StartPoint);

            for (double t = 0; t <= 1; t += 0.001)
            {
                PointD point = new PointD(GetCubicAtT(t, StartPoint.X, ControlPoint1.X, ControlPoint2.X, EndPoint.X), GetCubicAtT(t, StartPoint.Y, ControlPoint1.Y, ControlPoint2.Y, EndPoint.Y));
                points.Add(point);
            }

            points.Add(EndPoint);

            approximated = points.ToArray();
            return approximated;
        }
    }
}
