using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Expression.Media.Effects;
using TocTiny;

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

        private void Button_Click_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState.Minimized;
        }

        private void Button_Click_2(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Button_Click_3(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }
        
        private void Window_Activated(object sender, EventArgs e)
        {
            TabCtl.Effect = null;
            Brush windowGlassBrush = new SolidColorBrush(Program.GetSystemColor());
            Tab.Background = windowGlassBrush;
            MianBorder.BorderBrush = windowGlassBrush;
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
        /// <summary>   
        /// 获取鼠标的坐标   
        /// </summary>   
        /// <param name="lpPoint">传址参数，坐标point类型</param>   
        /// <returns>获取成功返回真</returns>   
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetCursorPos(out System.Drawing.Point pt);
        private static Point GetCursorPos()
        {
            System.Drawing.Point pt;
            GetCursorPos(out pt);
            return new Point(pt.X,pt.Y);
        }
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((!MianBorder.Child.IsMouseOver) && e.LeftButton == MouseButtonState.Pressed)
            {
                if (BottomRect.IsMouseOver)
                {
                    if (BottomRect.Tag != null) throw new Exception("LeftRect的Tag被占用");
                    DispatcherTimer dt = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Send, (s, e1) =>
                    {
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            Height = GetCursorPos().Y - Top;
                        }
                        else
                        {
                            if (BottomRect.Tag is DispatcherTimer)
                            {
                                ((DispatcherTimer)BottomRect.Tag).Stop();
                            }
                            if (LeftRect.Tag is DispatcherTimer)
                            {
                                ((DispatcherTimer)LeftRect.Tag).Stop();
                            }
                            BottomRect.Tag = null;
                            LeftRect.Tag = null;
                        }
                    },Dispatcher);
                    dt.Start();
                    BottomRect.Tag = dt;
                }
                else if (LeftRect.IsMouseOver)
                {
                    if (LeftRect.Tag != null) throw new Exception("LeftRect的Tag被占用");
                    DispatcherTimer dt = new DispatcherTimer(TimeSpan.FromMilliseconds(10),DispatcherPriority.Send, (s, e1) =>
                    {
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            Width = GetCursorPos().X - Left;
                        }
                        else
                        {
                            if (BottomRect.Tag is DispatcherTimer)
                            {
                                ((DispatcherTimer)BottomRect.Tag).Stop();
                            }
                            if (LeftRect.Tag is DispatcherTimer)
                            {
                                ((DispatcherTimer)LeftRect.Tag).Stop();
                            }
                            BottomRect.Tag = null;
                            LeftRect.Tag = null;
                        }
                    },Dispatcher);
                    dt.Start();
                    LeftRect.Tag = dt;
                }
            }
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((!MianBorder.Child.IsMouseOver) && e.LeftButton == MouseButtonState.Pressed)
            {
                if (BottomRect.Tag is DispatcherTimer)
                {
                    ((DispatcherTimer)BottomRect.Tag).Stop();
                }
                if (LeftRect.Tag is DispatcherTimer)
                {
                    ((DispatcherTimer)LeftRect.Tag).Stop();
                }
                BottomRect.Tag = null;
                LeftRect.Tag = null;
            }
        }
    }
}
