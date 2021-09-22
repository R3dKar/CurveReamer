using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace CurveUnfolder
{
    public class SvgPoint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected || IsStarting || IsEnding;
            set
            {
                if (isSelected == value || IsStarting || IsEnding)
                    return;
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(Stroke));
                OnPropertyChanged(nameof(Fill));
                OnPropertyChanged(nameof(Description));
            }
        }

        private bool isStarting;
        public bool IsStarting
        {
            get => isStarting;
            set
            {
                if (isStarting == value)
                    return;
                isStarting = value;
                OnPropertyChanged(nameof(IsStarting));
                OnPropertyChanged(nameof(Stroke));
                OnPropertyChanged(nameof(Fill));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(PointPath));
                OnPropertyChanged(nameof(SelectingEnabled));
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private bool isEnding;
        public bool IsEnding
        {
            get => isEnding;
            set
            {
                if (isEnding == value)
                    return;
                isEnding = value;
                OnPropertyChanged(nameof(IsEnding));
                OnPropertyChanged(nameof(Stroke));
                OnPropertyChanged(nameof(Fill));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(PointPath));
                OnPropertyChanged(nameof(SelectingEnabled));
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private double length;
        public double Length
        {
            get => length;
            set
            {
                if (length == value)
                    return;
                length = value;
                OnPropertyChanged(nameof(Length));
            }
        }

        private double x;
        public double X
        {
            get => x;
            set
            {
                if (x == value)
                    return;
                x = value;
                OnPropertyChanged(nameof(X));
                OnPropertyChanged(nameof(PointPath));
                OnPropertyChanged(nameof(PointInfo));
            }
        }

        private double y;
        public double Y
        {
            get => y;
            set
            {
                if (y == value)
                    return;
                y = value;
                OnPropertyChanged(nameof(Y));
                OnPropertyChanged(nameof(PointPath));
                OnPropertyChanged(nameof(PointInfo));
            }
        }

        private static SvgPoint instance = new SvgPoint();
        public static SvgPoint Instance
        {
            get => instance;
        }

        private static List<SvgPoint> instances = new List<SvgPoint>();
        public static List<SvgPoint> Instances { get => instances; }

        private static double pointSize = Properties.Settings.Default.PointSize;
        public double PointSize
        {
            get => pointSize;
            set
            {
                if (pointSize == value)
                    return;
                pointSize = value;
                Properties.Settings.Default.PointSize = value;
                Properties.Settings.Default.Save();
                instance.OnPropertyChanged(nameof(PointSize));
                foreach (var point in instances)
                {
                    point.OnPropertyChanged(nameof(PointPath));
                    point.OnPropertyChanged(nameof(PointThickness));
                }
            }
        }

        private static double visualizationScaling = 1;
        public static double VisualizationScaling
        {
            get => visualizationScaling;
            set
            {
                if (visualizationScaling == value)
                    return;
                visualizationScaling = value;
                instance.OnPropertyChanged(nameof(VisualizationScaling));
                foreach (var point in instances)
                {
                    point.OnPropertyChanged(nameof(VisualizationScaling));
                    point.OnPropertyChanged(nameof(PointPath));
                    point.OnPropertyChanged(nameof(PointThickness));
                }
            }
        }

        public double PointThickness => pointSize * 0.1;
        public Geometry PointPath => Geometry.Parse($"M {(X * VisualizationScaling - (IsEnding ? pointSize * 0.5 : pointSize) + 2).ToString("0.000", CultureInfo.InvariantCulture)} {(Y * VisualizationScaling - (IsEnding ? pointSize * 0.5 : pointSize) + 2).ToString("0.000", CultureInfo.InvariantCulture)} h {(IsEnding ? pointSize : pointSize + pointSize).ToString("0.000", CultureInfo.InvariantCulture)} v {(IsEnding ? pointSize : pointSize + pointSize).ToString("0.000", CultureInfo.InvariantCulture)} h -{(IsEnding ? pointSize : pointSize + pointSize).ToString("0.000", CultureInfo.InvariantCulture)} z");

        public Brush Stroke => IsStarting ? Brushes.Blue : IsEnding ? Brushes.Red : Brushes.Green;
        public Brush Fill => IsStarting ? Brushes.Blue : IsEnding ? Brushes.Red : IsSelected ? Brushes.Green : Brushes.Transparent;

        public bool SelectingEnabled => !(IsEnding || IsStarting);

        public string Description => IsStarting ? "Начало кривой" : IsEnding ? "Конец кривой" : "Точка (" + (IsSelected ? "выбрана" : "не выбрана") + ")";

        public string PointInfo => $"X: {X:0.00}; Y:{Y:0.00}";

        public SvgPoint(PointD point)
        {
            X = point.X;
            Y = point.Y;
            isSelected = true;
            instances.Add(this);
        }

        private SvgPoint() { }
    }
}
