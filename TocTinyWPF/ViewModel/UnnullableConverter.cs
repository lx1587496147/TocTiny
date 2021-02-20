using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TocTinyWPF.ViewModel
{
    internal class UnnullableConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return DependencyProperty.UnsetValue;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return DependencyProperty.UnsetValue;
            }

            return value;
        }
    }
}
