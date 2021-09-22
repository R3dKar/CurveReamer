using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CurveUnfolder
{
    public class PathLineSegment : PathSegment
    {
        public PathLineSegment(PointD startPoint, PointD endPoint, bool isReletive = false)
        {
            StartPoint = startPoint;
            if (isReletive)
                EndPoint = startPoint + endPoint;
            else
                EndPoint = endPoint;
        }
        
        public override PathSegment Clone()
        {
            return new PathLineSegment(StartPoint, EndPoint);
        }

        public override double GetLength()
        {
            return Math.Sqrt((StartPoint.X - EndPoint.X) * (StartPoint.X - EndPoint.X) + (StartPoint.Y - EndPoint.Y) * (StartPoint.Y - EndPoint.Y));
        }

        public override double[] GetSize()
        {
            return new double[4] { Math.Min(StartPoint.X, EndPoint.X), Math.Min(StartPoint.Y, EndPoint.Y), Math.Max(StartPoint.X, EndPoint.X), Math.Max(StartPoint.Y, EndPoint.Y) };
        }

        public override void Scale(double scale)
        {
            StartPoint *= scale;
            EndPoint *= scale;
        }

        public override void Translate(double x, double y)
        {
            PointD offset = new PointD(x, y);
            StartPoint += offset;
            EndPoint += offset;
        }

        public override PointD[] Approximate()
        {
            return new PointD[] { StartPoint, EndPoint };
        }
    }
}
