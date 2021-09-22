namespace CurveUnfolder
{
    public struct PointD
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PointD(double xy)
        {
            X = xy;
            Y = xy;
        }
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
        public PointD(PointD point)
        {
            X = point.X;
            Y = point.Y;
        }

        public static PointD operator +(PointD left, PointD right)
        {
            return new PointD(left.X + right.X, left.Y + right.Y);
        }
        public static PointD operator -(PointD left, PointD right)
        {
            return new PointD(left.X - right.X, left.Y - right.Y);
        }

        public static PointD operator *(PointD left, double right)
        {
            return new PointD(left.X * right, left.Y * right);
        }

        public override string ToString()
        {
            return $"{X}; {Y}";
        }
    }
}
