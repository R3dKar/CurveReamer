using System;
using System.Collections.Generic;
using System.Drawing;

namespace CurveReamer
{
    public class PathQuadraticBezierSegment : PathSegment
    {
        public PointD ControlPoint { get; set; }

        public PathQuadraticBezierSegment(PointD startPoint, PointD controlPoint, PointD endPoint, bool relative = false)
        {
            StartPoint = startPoint;
            if (relative)
            {
                ControlPoint = new PointD(startPoint.X + controlPoint.X, startPoint.Y + controlPoint.Y);
                EndPoint = new PointD(startPoint.X + endPoint.X, startPoint.Y + endPoint.Y);
            }
            else
            {
                ControlPoint = controlPoint;
                EndPoint = endPoint;
            }
        }
        public override PathSegment Clone()
        {
            return new PathQuadraticBezierSegment(StartPoint, ControlPoint, EndPoint);
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

        private static double GetQuadraticAtT(double t, double start, double c, double end)
        {
            double l1 = Lerp(start, c, t);
            double l2 = Lerp(c, end, t);
            return Lerp(l1, l2, t);
        }

        public override void Scale(double scale)
        {
            StartPoint *= scale;
            ControlPoint *= scale;
            EndPoint *= scale;
            approximated = null;
        }

        public override void Translate(double x, double y)
        {
            PointD offset = new PointD(x, y);
            StartPoint += offset;
            ControlPoint += offset;
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
                PointD point = new PointD(GetQuadraticAtT(t, StartPoint.X, ControlPoint.X, EndPoint.X), GetQuadraticAtT(t, StartPoint.Y, ControlPoint.Y, EndPoint.Y));
                points.Add(point);
            }

            points.Add(EndPoint);

            approximated = points.ToArray();
            return approximated;
        }
    }
}
