using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            MainChatViewModel viewModel = new MainChatViewModel();
            viewModel.RemoteServer = ViewModel.RemoteServer;
            viewModel.NickName = ViewModel.NickName;
            viewModel.ServerPort = ViewModel.ServerPort;
            viewModel.Password = ViewModel.Password;
            chat.DataContext = viewModel;
            (Application.Current.MainWindow.Content as Frame).Navigate(chat);
        }
    }
}
