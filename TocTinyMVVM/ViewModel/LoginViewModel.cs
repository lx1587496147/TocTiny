using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TocTiny.Lib;

namespace TocTiny.ViewModel
{
    class LoginViewModel : DependencyObject, INotifyPropertyChanged
    {
        private int remotePort;
        public int RemotePort
        {
            get
            {
                return remotePort;
            }
            set
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(RemotePort)));
                remotePort = value;
                if (!(remotePort < 0 | remotePort > 65535))
                {
                    throw new ArgumentOutOfRangeException(nameof(RemotePort));
                }
            }
        }
        private string password;
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
                password = value;
            }
        }
        private string nickName;
        public string NickName
        {
            get
            {
                return nickName;
            }
            set
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NickName)));
                nickName = value;
            }
        }
        private IPAddress serverAddress = IPAddress.Loopback;
        public string ServerAddress
        {
            get
            {
                return serverAddress.ToString();
            }
            set
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ServerAddress)));
                serverAddress = IPAddress.Parse(value);
            }
        }
        public ICommand ConnectClick
        {
            get
            {
                return new EventCommand((s,e) => { });
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
