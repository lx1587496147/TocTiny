using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace TocTinyWPF.ViewModel
{
    class IPAddressConverter :DependencyObject, IValueConverter
    {
        private static Heijden.DNS.Resolver resolver = new Heijden.DNS.Resolver();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            IPAddress ip = null;
            try
            {
                ip = resolver.Resolve(str).AddressList.First();
                return ip;
            }
            catch
            { }
            return DependencyProperty.UnsetValue;
        }
    }
}
