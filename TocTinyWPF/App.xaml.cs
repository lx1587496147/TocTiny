using System.Windows;

namespace TocTinyWPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        private void Application_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            if (!e.Handled)
            {
                MessageBox.Show(e.Exception.ToString());
                Shutdown(e.Exception.HResult);
            }
        }
    }
}
