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
using TocTinyWPF.Model;
using TocTinyWPF.ViewModel;

namespace TocTinyWPF.View
{
    /// <summary>
    /// MainChar.xaml 的交互逻辑
    /// </summary>
    public partial class MainChat : Page
    {
        public MainChat()
        {
            InitializeComponent();
        }
        private TocTinyModel model;
        private MainChatViewModel viewModel;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = ((MainChatViewModel)DataContext);
            try
            {
                viewModel.Msgs = Msgs;
            }
            catch
            { }
            model = new TocTinyModel(viewModel.RemoteServer, viewModel.ServerPort,viewModel.NickName, viewModel);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TransportPackage package = model.DefaultPackage;
            package.Content = viewModel.SendText;
            package.PackageType = (int)PackageType.NormalMessage;
            model.SendMessage(package);
        }
    }
}
