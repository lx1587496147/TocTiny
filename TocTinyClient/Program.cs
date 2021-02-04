using System;
using System.Windows;
using TocTinyClient;

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

        public static FrameHost GetMianWindow()
        {
            return (App.Current.MainWindow as FrameHost);
        }
        public static void a()
        {
            FrameHost framehost = new FrameHost() { Height=114514,Title="下北泽高速电车"};
            framehost.ShowDialog();
            framehost.Close();
        }
        public static void n()
        {
            FrameHost framehost = new FrameHost();
            framehost.Height = 114514;
            framehost.Title = "下北泽高速电车";
            framehost.ShowDialog();
            framehost.Close();
        }
    }
}
