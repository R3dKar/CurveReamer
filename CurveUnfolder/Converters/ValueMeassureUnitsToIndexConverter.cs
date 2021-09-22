using System;
using System.Globalization;
using System.Windows.Data;

namespace CurveUnfolder
{
    class ValueMeassureUnitsToIndexConverter : IValueConverter
    {
        private static ValueMeassureUnitsToIndexConverter instance;
        public static ValueMeassureUnitsToIndexConverter Instance
        {
            get => instance ?? (instance = new ValueMeassureUnitsToIndexConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(ValueMeassureUnits)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ValueMeassureUnits)(int)value;
        }
    }
}

