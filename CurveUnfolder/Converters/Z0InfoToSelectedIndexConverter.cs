using System;
using System.Globalization;
using System.Windows.Data;

namespace CurveUnfolder
{
    class Z0InfoToSelectedIndexConverter : IValueConverter
    {
        private static Z0InfoToSelectedIndexConverter instance;
        public static Z0InfoToSelectedIndexConverter Instance
        {
            get => instance ?? (instance = new Z0InfoToSelectedIndexConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(Z0Info)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Z0Info)(int)value;
        }
    }
}
