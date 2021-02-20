using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Data;

namespace TocTinyWPF.ViewModel
{
    internal class IPAddressConverter : DependencyObject, IValueConverter
    {
        private static readonly Heijden.DNS.Resolver resolver = new Heijden.DNS.Resolver();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            IPAddress ip;
            try
            {
                ip = IPAddress.Parse(str);
                return ip;
            }
            catch { }
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
