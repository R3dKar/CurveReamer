using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media;
using System;

namespace CurveUnfolder
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Window owner;
        public Window Owner
        {
            get => owner;
            set
            {
                if (owner == value)
                    return;
                owner = value;
                owner.Closing += (s, a) => SaveSettings();
                OnPropertyChanged(nameof(Owner));
            }
        }

        private SvgDocument svgFile;
        public SvgDocument SvgFile
        {
            get => svgFile;
            set
            {
                if (svgFile == value)
                    return;
                svgFile = value;
                OnPropertyChanged(nameof(SvgFile));
                OnPropertyChanged(nameof(PathIndexes));
                OnPropertyChanged(nameof(DocumentArea));
                OnPropertyChanged(nameof(CalculationsAllowed));
                OnPropertyChanged(nameof(NotAllowedHint));
                OnPropertyChanged(nameof(ResultValue));
                ChangeCurve(true);
                ExportCommand.TriggerCanExecuteChanged();
            }
        }

        #region Area and Value Properties
        public string DocumentArea
        {
            get
            {
                if (SvgFile == null)
                    return "0.000";

                string a = SvgFile.Area.ToString("0.000");

                switch (SvgFile.DocumentUnits)
                {
                    case MeasureUnits.Centimeters:
                        a += " см²";
                        break;
                    case MeasureUnits.Millimeters:
                        a += " мм²";
                        break;
                    case MeasureUnits.Inches:
                        a += " дюйм²";
                        break;
                    case MeasureUnits.Pixels:
                        a += " пикселей";
                        break;
                }

                return a;
            }
        }

        public bool CalculationsAllowed
        {
            get
            {
                if (SvgFile == null)
                    return false;

                return SvgFile.DocumentUnits != MeasureUnits.Undefined && SvgFile.DocumentUnits != MeasureUnits.Pixels;
            }
        }
        public string NotAllowedHint => CalculationsAllowed ? null : "Используются нематериальные\nединицы измерения";

        private double floorHeight;
        public double FloorHeight
        {
            get => floorHeight;
            set
            {
                if (floorHeight == value)
                    return;
                floorHeight = value;
                OnPropertyChanged(nameof(FloorHeight));
                OnPropertyChanged(nameof(ResultValue));
            }
        }

        private ValueMeassureUnits valueUnits;
        public ValueMeassureUnits ValueUnits
        {
            get => valueUnits;
            set
            {
                if (valueUnits == value)
                    return;
                valueUnits = value;
                OnPropertyChanged(nameof(ValueUnits));
                OnPropertyChanged(nameof(ResultValue));
            }
        }

        public string ResultValue
        {
            get
            {
                if (SvgFile == null || !CalculationsAllowed)
                    return "0.000";

                double mmArea = SvgFile.Area;
                switch (SvgFile.DocumentUnits)
                {
                    case MeasureUnits.Inches:
                        mmArea *= 645.16;
                        break;
                    case MeasureUnits.Centimeters:
                        mmArea *= 100;
                        break;
                    case MeasureUnits.Millimeters:
                        break;
                    default:
                        return "0.000";
                }

                switch (ValueUnits)
                {
                    case ValueMeassureUnits.Liter:
                        return (mmArea * FloorHeight * 0.000001).ToString("0.000");
                    case ValueMeassureUnits.Milliliter:
                    default:
                        return (mmArea * FloorHeight * 0.001).ToString("0.000");
                }
            }
        }
        #endregion

        #region Curve Properties
        private int pathIndex;
        public int PathIndex
        {
            get => pathIndex;
            set
            {
                if ((pathIndex == value && value != 0) || value >= SvgFile.Paths.Count)
                    return;
                pathIndex = value;
                OnPropertyChanged(nameof(PathIndex));
                ChangeCurve(false);
            }
        }
        public List<int> PathIndexes
        {
            get
            {
                var list = new List<int>();
                if (SvgFile != null)
                    for (int i = 1; i <= SvgFile.Paths.Count; i++)
                        list.Add(i);
                return list;
            }
        }
        public Geometry PathGeometry => GetFakeScaledPath(SvgFile?.Paths[PathIndex]);
        public string PathLength
        {
            get
            {
                if (SvgFile == null)
                    return "0.000";

                string l = SvgFile.Paths[PathIndex].GetLength().ToString("0.000");
                switch (SvgFile.DocumentUnits)
                {
                    case MeasureUnits.Pixels:
                        l += " пикселей";
                        break;
                    case MeasureUnits.Millimeters:
                        l += " мм";
                        break;
                    case MeasureUnits.Centimeters:
                        l += " см";
                        break;
                    case MeasureUnits.Inches:
                        l += " дюйм";
                        break;
                }
                return l;
            }
        }
        #endregion

        #region Points Properties
        private ObservableCollection<SvgPoint> points;
        public ObservableCollection<SvgPoint> Points
        {
            get => points;
            set
            {
                if (points == value)
                    return;
                points = value;
                OnPropertyChanged(nameof(Points));
            }
        }
        public Visibility IsPointsEmpty => (Points == null || Points.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
        #endregion

        #region Visualiation Properties
        private double pathThickness;
        public double PathThickness
        {
            get => pathThickness;
            set
            {
                if (pathThickness == value)
                    return;
                pathThickness = value;
                Properties.Settings.Default.PathThickness = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged(nameof(PathThickness));
            }
        }
        #endregion

        #region GCode Properties
        private double ySafe;
        public double YSafe
        {
            get => ySafe;
            set
            {
                if (ySafe == value)
                    return;
                ySafe = value;
                OnPropertyChanged(nameof(YSafe));
            }
        }

        private double lineHeight;
        public double LineHeight
        {
            get => lineHeight;
            set
            {
                if (lineHeight == value)
                    return;
                lineHeight = value;
                OnPropertyChanged(nameof(LineHeight));
            }
        }

        private double lineWidth;
        public double LineWidth
        {
            get => lineWidth;
            set
            {
                if (lineWidth == value)
                    return;
                lineWidth = value;
                OnPropertyChanged(nameof(LineWidth));
            }
        }

        private Z0Info cutBehaviour;
        public Z0Info CutBehaviour
        {
            get => cutBehaviour;
            set
            {
                if (cutBehaviour == value)
                    return;
                cutBehaviour = value;
                OnPropertyChanged(nameof(CutBehaviour));
            }
        }

        private double cutHeight;
        public double CutHeight
        {
            get => cutHeight;
            set
            {
                if (cutHeight == value)
                    return;
                cutHeight = value;
                OnPropertyChanged(nameof(CutHeight));
            }
        }

        private double widthOffset;
        public double WidthOffset
        {
            get => widthOffset;
            set
            {
                if (widthOffset == value)
                    return;
                widthOffset = value;
                OnPropertyChanged(nameof(WidthOffset));
            }
        }

        private bool doCutAtEnd;
        public bool DoCutAtEnd
        {
            get => doCutAtEnd;
            set
            {
                if (doCutAtEnd == value)
                    return;
                doCutAtEnd = value;
                OnPropertyChanged(nameof(DoCutAtEnd));
            }
        }
        #endregion

        #region Commands
        private RelayCommand importCommand;
        public RelayCommand ImportCommand
        {
            get => importCommand;
            set
            {
                if (importCommand == value)
                    return;
                importCommand = value;
                OnPropertyChanged(nameof(ImportCommand));
            }
        }
        private RelayCommand exportCommand;
        public RelayCommand ExportCommand
        {
            get => exportCommand;
            set
            {
                if (exportCommand == value)
                    return;
                exportCommand = value;
                OnPropertyChanged(nameof(ExportCommand));
            }
        }
        private RelayCommand exitCommand;
        public RelayCommand ExitCommand
        {
            get => exitCommand;
            set
            {
                if (exitCommand == value)
                    return;
                exitCommand = value;
                OnPropertyChanged(nameof(ExitCommand));
            }
        }
        private RelayCommand resetCommand;
        public RelayCommand ResetCommand
        {
            get => resetCommand;
            set
            {
                if (resetCommand == value)
                    return;
                resetCommand = value;
                OnPropertyChanged(nameof(ResetCommand));
            }
        }
        private RelayCommand openGCodeSettingsCommand;
        public RelayCommand OpenGCodeSettingsCommand
        {
            get => openGCodeSettingsCommand;
            set
            {
                if (openGCodeSettingsCommand == value)
                    return;
                openGCodeSettingsCommand = value;
                OnPropertyChanged(nameof(OpenGCodeSettingsCommand));
            }
        }
        private RelayCommand showAboutProgrammCommand;
        public RelayCommand ShowAboutProgrammCommand
        {
            get => showAboutProgrammCommand;
            set
            {
                if (showAboutProgrammCommand == value)
                    return;
                showAboutProgrammCommand = value;
                OnPropertyChanged(nameof(ShowAboutProgrammCommand));
            }
        }
        #endregion

        public MainWindowViewModel()
        {
            ImportCommand = new RelayCommand(Import);
            ExportCommand = new RelayCommand(Export, () => SvgFile != null);
            ExitCommand = new RelayCommand(Exit);
            ResetCommand = new RelayCommand(Reset);
            OpenGCodeSettingsCommand = new RelayCommand(SetUpGCode);
            ShowAboutProgrammCommand = new RelayCommand(ShowAboutProgram);
            Points = new ObservableCollection<SvgPoint>();
            Points.CollectionChanged += (s, args) => OnPropertyChanged(nameof(IsPointsEmpty));

            YSafe = Properties.Settings.Default.YSafe;
            LineHeight = Properties.Settings.Default.LineHeight;
            LineWidth = Properties.Settings.Default.LineWidth;
            CutBehaviour = Properties.Settings.Default.CutBehaviour;
            CutHeight = Properties.Settings.Default.CutHeight;
            WidthOffset = Properties.Settings.Default.WidthOffset;
            PathThickness = Properties.Settings.Default.PathThickness;
            DoCutAtEnd = Properties.Settings.Default.DoCutAtEnd;
            ValueUnits = Properties.Settings.Default.ValueUnits;
            FloorHeight = Properties.Settings.Default.FloorHeight;
        }

        private void Import()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.CheckFileExists = true;
            openDialog.AddExtension = true;
            openDialog.Filter = "SVG (*.svg)|*.svg|Все файлы (*.*)|*.*";

            if (openDialog.ShowDialog(Owner) == true)
            {
                var svg = new SvgDocument();
                svg.Load(openDialog.FileName);

                if (svg.Paths.Count == 0)
                    MessageBox.Show(Owner, "Кривые не найдены!", "Ошибка чтения файла", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                else
                    SvgFile = svg;
            }
        }

        private void Export()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.AddExtension = true;
            saveDialog.Filter = "CNC (*.cnc)|*.cnc";

            if (saveDialog.ShowDialog(Owner) == true)
            {
                string gcode = Properties.Settings.Default.StartingGCode + "\n";
                string left = (-WidthOffset).ToString("0.000", CultureInfo.InvariantCulture);
                string right = (LineWidth + WidthOffset).ToString("0.000", CultureInfo.InvariantCulture);
                string top = YSafe.ToString("0.000", CultureInfo.InvariantCulture);
                string bottom = string.Empty;
                string cut = string.Empty;
                switch (CutBehaviour)
                {
                    case Z0Info.Line:
                        bottom = (-CutHeight).ToString("0.000", CultureInfo.InvariantCulture);
                        cut = (-LineHeight).ToString("0.000", CultureInfo.InvariantCulture);
                        break;
                    case Z0Info.Table:
                        bottom = (LineHeight - CutHeight).ToString("0.000", CultureInfo.InvariantCulture);
                        cut = "0";
                        break;
                }

                bool isRightSide = false;
                bool firstPoint = true;
                double y = 0;

                gcode += $"G0 Z{top}\n";

                foreach (var point in Points)
                {
                    y += point.Length;
                    if (point.IsSelected && !point.IsStarting)
                    {
                        if (firstPoint)
                        {
                            gcode += $"G0 X{left} Y{y.ToString("0.000", CultureInfo.InvariantCulture)}\n";
                            gcode += $"G1 Z{bottom}\n";
                            firstPoint = false;
                        }
                        else if (point.IsEnding && DoCutAtEnd)
                            gcode += $"G1 Y{y.ToString("0.000", CultureInfo.InvariantCulture)} Z{cut}\n";
                        else
                            gcode += $"G1 Y{y.ToString("0.000", CultureInfo.InvariantCulture)}\n";
                        gcode += $"G1 X{(isRightSide ? left : right)}\n";
                        isRightSide = !isRightSide;
                    }
                }

                gcode += $"G1 Z{top}\n";
                gcode += $"G0 X0 Y0\n";
                gcode += Properties.Settings.Default.EndingGCode;

                string[] lines = gcode.Split('\n');
                gcode = string.Empty;
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        gcode += line + "\n";
                }

                File.WriteAllText(saveDialog.FileName, gcode);
            }
        }

        private void Exit()
        {
            SaveSettings();
            Application.Current.Shutdown();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.YSafe = YSafe;
            Properties.Settings.Default.LineHeight = LineHeight;
            Properties.Settings.Default.LineWidth = LineWidth;
            Properties.Settings.Default.CutBehaviour = CutBehaviour;
            Properties.Settings.Default.CutHeight = CutHeight;
            Properties.Settings.Default.WidthOffset = WidthOffset;
            Properties.Settings.Default.PathThickness = PathThickness;
            Properties.Settings.Default.DoCutAtEnd = DoCutAtEnd;
            Properties.Settings.Default.ValueUnits = ValueUnits;
            Properties.Settings.Default.FloorHeight = FloorHeight;
            Properties.Settings.Default.Save();
        }

        private void Reset()
        {
            string s = Properties.Settings.Default.StartingGCode;
            string e = Properties.Settings.Default.EndingGCode;
            Properties.Settings.Default.Reset();

            YSafe = Properties.Settings.Default.YSafe;
            LineHeight = Properties.Settings.Default.LineHeight;
            LineWidth = Properties.Settings.Default.LineWidth;
            CutBehaviour = Properties.Settings.Default.CutBehaviour;
            CutHeight = Properties.Settings.Default.CutHeight;
            WidthOffset = Properties.Settings.Default.WidthOffset;
            PathThickness = Properties.Settings.Default.PathThickness;
            DoCutAtEnd = Properties.Settings.Default.DoCutAtEnd;
            ValueUnits = Properties.Settings.Default.ValueUnits;
            FloorHeight = Properties.Settings.Default.FloorHeight;
            SvgPoint.Instance.PointSize = Properties.Settings.Default.PointSize;

            Properties.Settings.Default.StartingGCode = s;
            Properties.Settings.Default.EndingGCode = e;
            Properties.Settings.Default.Save();
        }

        private void SetUpGCode()
        {
            new GCodeSettingsWindow(Owner).ShowDialog();
        }

        private void ShowAboutProgram()
        {
            new AboutWindow(Owner).ShowDialog();
        }

        private void ChangeCurve(bool isFromFile)
        {
            if (isFromFile)
                PathIndex = 0;
            OnPropertyChanged(nameof(PathGeometry));
            OnPropertyChanged(nameof(PathLength));

            Points.Clear();
            SvgPoint.Instances.Clear();

            SvgPoint.VisualizationScaling = GetVisualizationScale(SvgFile.Paths[PathIndex]);

            Points.Add(new SvgPoint(SvgFile.Paths[PathIndex].Segments.First().StartPoint) { IsStarting = true });

            foreach (var segment in SvgFile.Paths[PathIndex].Segments)
            {
                Points.Add(new SvgPoint(segment.EndPoint) { Length = segment.GetLength(), IsEnding = segment == SvgFile.Paths[PathIndex].Segments.Last() });
            }
        }

        public void SetStartingPoint(SvgPoint point)
        {
            if (!SvgFile.Paths[PathIndex].Closed)
                return;

            if (point.IsEnding || point.IsStarting)
                return;

            int start = Points.IndexOf(point);
            List<SvgPoint> newOrder = new List<SvgPoint>();
            for (int i = start + 1; i < Points.Count; i++)
            {
                newOrder.Add(Points[i]);
            }
            for (int i = 1; i <= start; i++)
            {
                newOrder.Add(Points[i]);
            }

            newOrder.Insert(0, Points.First());
            newOrder[0].X = point.X;
            newOrder[0].Y = point.Y;
            foreach (var p in newOrder)
            {
                if (p.IsStarting)
                    continue;

                p.IsEnding = false;
            }
            newOrder.Last().IsEnding = true;

            Points.Clear();
            foreach (var p in newOrder)
                Points.Add(p);
        }

        public double GetVisualizationScale(Path path)
        {
            if (path == null)
                return 1;

            double[] size = path.GetSize();
            double scale = 200;
            if (size[2] - size[0] > size[3] - size[1])
                scale /= size[2] - size[0];
            else
                scale /= size[3] - size[1];

            return scale;
        }

        public Geometry GetFakeScaledPath(Path path)
        {
            if (path == null)
                return Geometry.Parse(null);

            Path p = path.Clone();
            p.Scale(GetVisualizationScale(path));
            p.Translate(2, 2);

            return Geometry.Parse(p.ToString());
        }
    }
}
