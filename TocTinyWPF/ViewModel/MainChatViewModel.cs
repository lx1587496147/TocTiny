using System.Net;
using System.Windows.Controls;

namespace TocTinyWPF.ViewModel
{
    [PropertyChanged.ImplementPropertyChanged]
    internal class MainChatViewModel
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
