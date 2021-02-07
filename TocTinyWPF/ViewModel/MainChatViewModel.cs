using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Controls;

namespace TocTinyWPF.ViewModel
{
    class MainChatViewModel
    {
        public IPAddress RemoteServer { get; set; } = IPAddress.Loopback;
        public ushort ServerPort { get; set; } = 2020;
        public string SendText { get; set; } = "";
        public string NickName { get; set; } = "Null";
        public string ChannelName { get; set; } = "NullStream";
        public StackPanel Msgs { get; set; } = new StackPanel() { Orientation = Orientation.Vertical };
        public string Password { get; set; }
    }
}
