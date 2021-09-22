namespace CurveReamer
{
    public abstract class PathSegment
    {
        public PointD StartPoint { get; set; }
        public PointD EndPoint { get; set; }

        public abstract PathSegment Clone();
        public abstract double GetLength();
        public abstract double[] GetSize();

        public abstract void Scale(double scale);
        public abstract void Translate(double x, double y);
        public abstract PointD[] Approximate();

    }
}
