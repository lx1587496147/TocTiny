using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TocTinyWPF.View;
using TocTinyWPF.ViewModel;

namespace TocTinyWPF.Model
{

    class TocTinyModel : DependencyObject
    {
        private NetworkService Network = new NetworkService();
        public dynamic ViewModel;
        public TocTinyModel(IPAddress ip,int port,string name,dynamic ViewModel)
        {
            this.ViewModel = ViewModel;
            DefaultPackage.Name = name;
            Network.Connect(ip,port);
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
                            MsgViewModel viewModel = new MsgViewModel();
                            viewModel.Publisher = package.Name;
                            viewModel.Content = new TextBlock(new Run(package.Content));
                            msg.DataContext = viewModel;
                            ViewModel.Msgs.Children.Add(msg);
                            break;
                        }
                    case PackageType.ImageMessage:
                        {
                            Msg msg = new Msg();
                            MsgViewModel viewModel = new MsgViewModel();
                            viewModel.Publisher = package.Name;
                            viewModel.Content = new TextBlock(new InlineUIContainer(new Image() { Source = BaseToImageSource(package.Content),Height=200 }));
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
            BitmapDecoder decoder = BitmapDecoder.Create(new MemoryStream(buffer),BitmapCreateOptions.DelayCreation,BitmapCacheOption.OnLoad);
            return decoder.Frames[0];
        }
        public TransportPackage DefaultPackage = new TransportPackage();
        public void SendMessage(TransportPackage package)
        {
            Network.Send(package);
        }
    }
}
