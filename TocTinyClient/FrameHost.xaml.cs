using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace TocTinyClient
{
    /// <summary>
    /// FrameHost.xaml 的交互逻辑
    /// </summary>
    public partial class FrameHost
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        protected override void OnSourceInitialized(EventArgs e)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SendMessage(hwnd, 0x7F, new IntPtr(1), IntPtr.Zero);
            SendMessage(hwnd, 0x7F, IntPtr.Zero, IntPtr.Zero);
            base.OnSourceInitialized(e); 
        }
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
    }
}
