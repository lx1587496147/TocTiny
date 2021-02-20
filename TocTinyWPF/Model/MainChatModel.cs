using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TocTinyWPF.View;
using TocTinyWPF.ViewModel;

namespace TocTinyWPF.Model
{
    internal class MainChatModel : DependencyObject
    {
        private readonly NetworkService Network = new NetworkService();
        public dynamic ViewModel;
        public MainChatModel(IPAddress ip, int port, string name, dynamic ViewModel,EventHandler NetworkFailedCallback)
        {
            NetworkFailed = (s, e) => { };
            this.ViewModel = ViewModel;
            DefaultPackage.Name = name;
            Network.NetworkFailed += (s, e) => OnNetworkFailed();
            NetworkFailed += NetworkFailedCallback;
            Network.Connect(ip, port);
            Network.Start();
            Network.PackageReviced += PackageReviced;
        }
        private void PackageReviced(object sender, Eventarg.PackageRevicedEventArgs e)
        {
            TransportPackage package = (TransportPackage)e.Package.Load;
            Dispatcher.Invoke((Action)(() =>
            {
                switch ((PackageType)(package.PackageType))
                {
                    case PackageType.NormalMessage:
                        {
                            Msg msg = new Msg();
                            MsgViewModel viewModel = new MsgViewModel
                            {
                                Publisher = package.Name,
                                Content = new TextBlock(new Run(package.Content))
                            };
                            msg.DataContext = viewModel;
                            ViewModel.Msgs.Children.Add(msg);
                            break;
                        }
                    case PackageType.ImageMessage:
                        {
                            Msg msg = new Msg();
                            MsgViewModel viewModel = new MsgViewModel
                            {
                                Publisher = package.Name,
                                Content = new TextBlock(new InlineUIContainer(new Image() { Source = BaseToImageSource(package.Content), Height = 200 }))
                            };
                            msg.DataContext = viewModel;
                            ViewModel.Msgs.Children.Add(msg);
                            break;
                        }
                    case PackageType.ChangeChannelName:
                        ViewModel.ChannelName = package.Content;
                        break;
                    case PackageType.Login:
                        DefaultPackage.ClientGuid = package.Content;
                        break;
                    default:
                        ViewModel.Msgs.Children.Add(new TextBlock(new Run(package.Content)));
                        break;
                }
            }));
        }
        private ImageSource BaseToImageSource(string base64)
        {
            byte[] buffer = Convert.FromBase64String(base64);
            BitmapDecoder decoder = BitmapDecoder.Create(new MemoryStream(buffer), BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
            return decoder.Frames[0];
        }
        public TransportPackage DefaultPackage = new TransportPackage();

        public event EventHandler NetworkFailed;
        private void OnNetworkFailed()
        {
            NetworkFailed.Invoke(this,new EventArgs());
        }
        public void SendMessage(TransportPackage package)
        {
            try
            {
                Network.Send(package);
            }
            catch
            {
                OnNetworkFailed();
            }
        }
    }
}
