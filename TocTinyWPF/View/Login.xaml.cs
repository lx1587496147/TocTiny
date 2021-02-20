using System.Windows;
using System.Windows.Controls;
using TocTinyWPF.ViewModel;

namespace TocTinyWPF.View
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainChat chat = new MainChat();
            MainChatViewModel viewModel = new MainChatViewModel
            {
                RemoteServer = ViewModel.RemoteServer,
                NickName = ViewModel.NickName,
                ServerPort = ViewModel.ServerPort,
                Password = ViewModel.Password
            };
            chat.DataContext = viewModel;
            (Application.Current.MainWindow.Content as Frame).Navigate(chat);
        }
    }
}
