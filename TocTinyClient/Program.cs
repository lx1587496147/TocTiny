using System;
using System.Windows;
using TocTinyClient;
using Microsoft.Win32;
using System.Windows.Media;

namespace TocTiny
{
    internal static class Program
    {
        [STAThread()]
        private static void Main()
        {
            if ((System.Windows.Media.RenderCapability.Tier >> 16)!=2)
            {
                MessageBox.Show("警告：此系统不支持硬件渲染");
            }
            App app = new App();
            FrameHost frameHost = new FrameHost();
            TocTinyClient.Theme.ThemeSwitcher.SwitchTheme(TocTinyClient.Theme.ThemeEnum.AERO, frameHost);
            frameHost.Loaded += (sender, e) => frameHost.Frame.Navigate(new Login());
            app.Exit += (s, e) => { Environment.Exit(0); };
            app.DispatcherUnhandledException +=
                (sender, e) =>
                {
                    TocErrorReport tocErrorReport = new TocErrorReport();
                    tocErrorReport.dzzz.Text = e.Exception.ToString();
                    frameHost.Frame.Navigate(tocErrorReport);
                    e.Handled = true;
                    return;
                };
            app.Run(frameHost);
        }
        internal static void Navigate(object page)
        {
            GetMianWindow().Frame.Navigate(page);
        }
        public static Color ConvertToColor(int value)
        {
            return Color.FromArgb(
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            );
        }
        public static Color GetSystemColor()
        {
            return ConvertToColor((int)Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\DWM").GetValue("AccentColor"));
        }
        public static FrameHost GetMianWindow()
        {
            return (App.Current.MainWindow as FrameHost);
        }
    }
}
