using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Microsoft.Expression.Media.Effects;

namespace TocTinyClient
{
    /// <summary>
    /// FrameHost.xaml 的交互逻辑
    /// </summary>
    public partial class FrameHost
    {
        public FrameHost()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            throw new Exception("测试异常");
        }
        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState.Minimized;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Frame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            
            if (e.Content is FrameworkElement)
            {
                Theme.ThemeSwitcher.SwitchTheme(Theme.ThemeEnum.AERO, (FrameworkElement)e.Content);
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            TabCtl.Effect = null;
            Tab.Background = SystemParameters.WindowGlassBrush;
            MianBorder.BorderBrush = SystemParameters.WindowGlassBrush;
        }
        private MonochromeEffect monochromeEffect = new MonochromeEffect
        {
            Color = Color.FromArgb(255, 99, 99, 99)
        };
        private void Window_Deactivated(object sender, EventArgs e)
        {
            TabCtl.Effect = monochromeEffect;
            Tab.Background = Brushes.White;
            MianBorder.BorderBrush =new SolidColorBrush( monochromeEffect.Color);
        }
    }
}
