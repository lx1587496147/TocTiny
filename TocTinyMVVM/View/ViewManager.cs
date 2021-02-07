using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace TocTiny.View
{
    partial class ViewManager : Application
    {
        private Window _MainWindow;
        private Frame frame;
        public ViewManager()
        {
            _MainWindow = new Window();
            _MainWindow.Content = frame = new Frame();
            _MainWindow.Loaded += (s,e) => Navigate(new Uri("/TocTiny;component/view/login.xaml",UriKind.Relative));
        }
        public new Uri StartupUri { get; set; }
        public bool Navigate(Uri uri, object arg = null) => arg == null ? frame.Navigate(uri) : frame.Navigate(uri, arg);
        public new void Run() => Run(_MainWindow);
    }
}
