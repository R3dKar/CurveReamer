using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Globalization;

namespace CurveReamer
{
    public class SvgDocument
    {
        public List<Path> Paths { get; private set; }
        public MeasureUnits DocumentUnits { get; private set; } = MeasureUnits.Undefined;
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double Area { get; private set; }

        public SvgDocument()
        {
            Paths = new List<Path>();
        }

        public SvgDocument(string filename)
        {
            Paths = new List<Path>();

            Load(filename);
        }

        public void Load(string filename)
        {
            Paths.Clear();

            XDocument xml = XDocument.Load(filename);
            List<string> paths = new List<string>();

            double scale = 1;

            foreach (var el in xml.Descendants())
            {
                if (el.Name.LocalName == "path")
                    paths.Add((string)el.Attribute("d"));
                else if (el.Name.LocalName == "svg")
                {
                    if (el.Attribute("width") != null)
                    {
                        Width = GetDimensionValue((string)el.Attribute("width"));
                        Match m = Regex.Match((string)el.Attribute("width"), "(mm|px|in|i|cm)");
                        switch (m.Value)
                        {
                            case "in":
                            case "i":
                                DocumentUnits = MeasureUnits.Inches;
                                break;
                            case "mm":
                                DocumentUnits = MeasureUnits.Millimeters;
                                break;
                            case "cm":
                                DocumentUnits = MeasureUnits.Centimeters;
                                break;
                            case "px":
                                DocumentUnits = MeasureUnits.Pixels;
                                break;
                            default:
                                DocumentUnits = MeasureUnits.Pixels;
                                break;
                        }
                    }

                    if (el.Attribute("height") != null)
                        Height = GetDimensionValue((string)el.Attribute("height"));

                    if (el.Attribute("viewBox") != null)
                    {
                        double[] dimensions = new double[4];
                        int added = 0;
                        foreach (var chunk in ((string)el.Attribute("viewBox")).Split(' ', '\n', '\t'))
                        {
                            if (string.IsNullOrWhiteSpace(chunk))
                                continue;

                            dimensions[added++] = double.Parse(chunk, NumberStyles.Float, CultureInfo.InvariantCulture);
                        }

                        if (DocumentUnits == MeasureUnits.Undefined)
                        {
                            Width = dimensions[2];
                            Height = dimensions[3];
                        }
                        else
                        {
                            scale = Width / dimensions[2];
                        }
                    }
                }
            }

            foreach (var path in paths)
                Paths.AddRange(Path.Parse(path));

            Path main = null;
            double[] mainDimensions = new double[4] { 0, 0, 0, 0 };
            foreach (var path in Paths)
            {
                double[] pathDimensions = path.GetSize();
                if ((mainDimensions[2] - mainDimensions[0]) * (mainDimensions[3] - mainDimensions[1]) < (pathDimensions[2] - pathDimensions[0]) * (pathDimensions[3] - pathDimensions[1]))
                {
                    main = path;
                    mainDimensions = pathDimensions;
                }
            }
            Area = main.GetArea(Paths) * scale * scale;

            foreach (var path in Paths)
                path.Scale(scale);

            foreach (var path in Paths)
            {
                double[] size = path.GetSize();
                path.Translate(-size[0], -size[1]);
            }
        }

        private static double GetDimensionValue(string value)
        {
            Match m = Regex.Match(value.ToLower(), @"(?<float>[+-]?([0-9]+((\.|,)[0-9]*)?|[.][0-9]+))\s*(mm|px|in|i|cm)?");
            if (m.Success)
                return double.Parse(m.Groups["float"].Value, NumberStyles.Float, CultureInfo.InvariantCulture);

            throw new ArgumentException();
        }
    }
}
