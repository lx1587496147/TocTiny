using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace TocTinyWPF.ViewModel
{
    class UnnullableConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value.ToString()))
                return DependencyProperty.UnsetValue;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value.ToString()))
                return DependencyProperty.UnsetValue;
            return value;
        }
    }
}
