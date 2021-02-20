using System.Windows;
using System.Windows.Controls;
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
        private MainChatModel model;
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
            model = new MainChatModel(viewModel.RemoteServer, viewModel.ServerPort, viewModel.NickName, viewModel, Model_NetworkFailed);
        }

        private void Model_NetworkFailed(object sender, System.EventArgs e)
        {
            Popup.Visibility= Visibility.Visible;
            Msgbox.Text = "网络连接丢失或中断";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TransportPackage package = model.DefaultPackage;
            package.Content = viewModel.SendText;
            package.PackageType = (int)PackageType.NormalMessage;
            model.SendMessage(package);
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow.Content as Frame).GoBack();
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && (e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.RigthAlt)))
            {
                Button_Click(sender, e);
            } 
         }
    }
}
