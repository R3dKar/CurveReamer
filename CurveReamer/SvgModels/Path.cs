using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Globalization;

namespace CurveUnfolder
{
    public class Path
    {
        public bool Closed { get; private set; } = false;
        public List<PathSegment> Segments { get; private set; }

        private Path()
        {
            Segments = new List<PathSegment>();
        }

        public static List<Path> Parse(string pathData)
        {
            List<Path> paths = new List<Path>();
            Path current = null;

            PointD lastPoint = new PointD(0, 0);
            List<double> doubleList = new List<double>();
            char lastCommand = 'M';
            bool isCommandRelative = false;

            string splitPattern = @"(m|M|z|Z|l|L|h|H|v|V|c|C|s|S|q|Q|t|T|a|A|;|,|\\c|\\s| )";
            string delimeterPattern = @"(;|,|\\c|\\s| )";
            string commands = "mMzZlLhHvVcCsSqQtTaA";

            foreach (string chunk in Regex.Split(pathData, splitPattern))
            {
                if (Regex.IsMatch(chunk, delimeterPattern) || string.IsNullOrWhiteSpace(chunk))
                    continue;

                if (commands.IndexOf(chunk) >= 0)
                {
                    lastCommand = chunk.Last();
                    isCommandRelative = char.IsLower(lastCommand);

                    if (char.ToLower(lastCommand) == 'z' && current != null)
                    {
                        if (Math.Abs(lastPoint.X - current.Segments.First().StartPoint.X) < 0.1 && Math.Abs(lastPoint.Y - current.Segments.First().StartPoint.Y) < 0.1)
                            current.Segments.Last().EndPoint = current.Segments.First().StartPoint;
                        else
                        {
                            current.Segments.Add(new PathLineSegment(lastPoint, current.Segments.First().StartPoint));
                        }
                        current.Closed = true;
                        lastPoint = current.Segments.First().StartPoint;
                        doubleList.Clear();
                    }
                    else if (char.ToLower(lastCommand) != 'm' && current != null)
                        current.Closed = false;
                }
                else if (double.TryParse(chunk, NumberStyles.Float, CultureInfo.InvariantCulture, out double n))
                {
                    doubleList.Add(n);

                    if (char.ToLower(lastCommand) == 'm' && doubleList.Count == 2)
                    {
                        if (current != null && current.Segments.Count != 0)
                            paths.Add(current);

                        current = new Path();
                        if (isCommandRelative)
                            lastPoint = new PointD(lastPoint.X + doubleList[0], lastPoint.Y + doubleList[1]);
                        else
                            lastPoint = new PointD(doubleList[0], doubleList[1]);
                        doubleList.Clear();
                    }
                    else if (char.ToLower(lastCommand) == 'l' && doubleList.Count == 2)
                    {
                        current.Segments.Add(new PathLineSegment(lastPoint, new PointD(doubleList[0], doubleList[1]), isCommandRelative));
                        lastPoint = current.Segments.Last().EndPoint;
                        doubleList.Clear();
                    }
                    else if (char.ToLower(lastCommand) == 'v' && doubleList.Count == 1)
                    {
                        current.Segments.Add(new PathLineSegment(lastPoint, new PointD(isCommandRelative ? 0 : lastPoint.X, doubleList[0]), isCommandRelative));
                        lastPoint = current.Segments.Last().EndPoint;
                        doubleList.Clear();
                    }
                    else if (char.ToLower(lastCommand) == 'h' && doubleList.Count == 1)
                    {
                        current.Segments.Add(new PathLineSegment(lastPoint, new PointD(doubleList[0], isCommandRelative ? 0 : lastPoint.Y), isCommandRelative));
                        lastPoint = current.Segments.Last().EndPoint;
                        doubleList.Clear();
                    }
                    else if (char.ToLower(lastCommand) == 'c' && doubleList.Count == 6)
                    {
                        current.Segments.Add(new PathCubicBezierSegment(lastPoint, new PointD(doubleList[0], doubleList[1]), new PointD(doubleList[2], doubleList[3]), new PointD(doubleList[4], doubleList[5]), isCommandRelative));
                        lastPoint = current.Segments.Last().EndPoint;
                        doubleList.Clear();
                    }
                    else if (char.ToLower(lastCommand) == 's' && doubleList.Count == 4)
                    {
                        PointD controlPoint1 = (current.Segments.Last() as PathCubicBezierSegment).ControlPoint2;
                        controlPoint1 = isCommandRelative ? lastPoint - controlPoint1 : lastPoint + lastPoint - controlPoint1;
                        current.Segments.Add(new PathCubicBezierSegment(lastPoint, controlPoint1, new PointD(doubleList[0], doubleList[1]), new PointD(doubleList[2], doubleList[3]), isCommandRelative));
                        lastPoint = current.Segments.Last().EndPoint;
                        doubleList.Clear();
                    }
                    else if (char.ToLower(lastCommand) == 'q' && doubleList.Count == 4)
                    {
                        current.Segments.Add(new PathQuadraticBezierSegment(lastPoint, new PointD(doubleList[0], doubleList[1]), new PointD(doubleList[2], doubleList[3]), isCommandRelative));
                        lastPoint = current.Segments.Last().EndPoint;
                        doubleList.Clear();
                    }
                    else if (char.ToLower(lastCommand) == 't' && doubleList.Count == 2)
                    {
                        PointD controlPoint = (current.Segments.Last() as PathQuadraticBezierSegment).ControlPoint;
                        controlPoint = isCommandRelative ? lastPoint - controlPoint : lastPoint + lastPoint - controlPoint;
                        current.Segments.Add(new PathQuadraticBezierSegment(lastPoint, controlPoint, new PointD(doubleList[0], doubleList[1]), isCommandRelative));
                        lastPoint = current.Segments.Last().EndPoint;
                        doubleList.Clear();
                    }
                    else if (char.ToLower(lastCommand) == 'a' && doubleList.Count == 7)
                        throw new NotImplementedException();
                }
            }

            if (current != null && current.Segments.Count != 0)
                paths.Add(current);

            return paths;
        }

        public void Scale(double scale)
        {
            foreach (var segment in Segments)
                segment.Scale(scale);
        }

        public void Translate(double x, double y)
        {
            foreach (var segment in Segments)
                segment.Translate(x, y);
        }

        public double GetLength()
        {
            double length = 0;
            foreach (var segment in Segments)
                length += segment.GetLength();

            return length;
        }

        public double[] GetSize()
        {
            double[] size = Segments.First().GetSize();
            foreach (var segment in Segments)
            {
                double[] current = segment.GetSize();
                size[0] = Math.Min(size[0], current[0]);
                size[1] = Math.Min(size[1], current[1]);
                size[2] = Math.Max(size[2], current[2]);
                size[3] = Math.Max(size[3], current[3]);
            }

            return size;
        }

        public double GetArea()
        {
            if (!Closed)
                return 0;

            List<PointD> points = new List<PointD>();
            foreach (var segment in Segments)
            {
                List<PointD> approximated = segment.Approximate().ToList();
                approximated.RemoveAt(0);
                points.AddRange(approximated);
            }

            double area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                area += (points[i].Y + points[(i + 1) % points.Count].Y) * (points[i].X - points[(i + 1) % points.Count].X) * 0.5; 
            }

            return Math.Abs(area);
        }

        public double GetArea(List<Path> figures)
        {
            List<Path> inner = new List<Path>();
            double[] thisSize = GetSize();
            double area = GetArea();

            foreach (var figure in figures)
            {
                if (figure == this || !figure.Closed)
                    continue;
                double[] figureSize = figure.GetSize();
                if (thisSize[0] <= figureSize[0] && thisSize[1] <= figureSize[1] && thisSize[2] >= figureSize[2] && thisSize[3] >= figureSize[3])
                {
                    inner.Add(figure);
                }

                area -= figure.GetArea(inner);
            }

            return area;
        }

        public override string ToString()
        {
            string pathString = $"M {Segments.First().StartPoint.X.ToString("0.000", CultureInfo.InvariantCulture)} {Segments.First().StartPoint.Y.ToString("0.000", CultureInfo.InvariantCulture)}";
            foreach (var segment in Segments)
            {
                if (segment is PathLineSegment)
                    pathString += $" L {segment.EndPoint.X.ToString("0.000", CultureInfo.InvariantCulture)} {segment.EndPoint.Y.ToString("0.000", CultureInfo.InvariantCulture)}";
                else if (segment is PathCubicBezierSegment)
                {
                    var bezier = segment as PathCubicBezierSegment;
                    pathString += $" C {bezier.ControlPoint1.X.ToString("0.000", CultureInfo.InvariantCulture)} {bezier.ControlPoint1.Y.ToString("0.000", CultureInfo.InvariantCulture)} {bezier.ControlPoint2.X.ToString("0.000", CultureInfo.InvariantCulture)} {bezier.ControlPoint2.Y.ToString("0.000", CultureInfo.InvariantCulture)} {bezier.EndPoint.X.ToString("0.000", CultureInfo.InvariantCulture)} {bezier.EndPoint.Y.ToString("0.000", CultureInfo.InvariantCulture)}";
                }
                else if (segment is PathQuadraticBezierSegment)
                {
                    var bezier = segment as PathQuadraticBezierSegment;
                    pathString += $" Q {bezier.ControlPoint.X.ToString("0.000", CultureInfo.InvariantCulture)} {bezier.ControlPoint.Y.ToString("0.000", CultureInfo.InvariantCulture)} {bezier.EndPoint.X.ToString("0.000", CultureInfo.InvariantCulture)} {bezier.EndPoint.Y.ToString("0.000", CultureInfo.InvariantCulture)}";
                }
            }

            if (Closed)
                pathString += " Z";

            return pathString;
        }

        public Path Clone()
        {
            Path p = new Path();
            foreach (var segment in Segments)
                p.Segments.Add(segment.Clone());

            return p;
        }
    }
}
