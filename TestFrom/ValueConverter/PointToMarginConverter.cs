using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DistanceSensorAppDemo.ValueConverter;

public class PointToMarginConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        double x = (double)values[0];
        double y = (double)values[1];
        return new Thickness(x, y, 0, 0);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}