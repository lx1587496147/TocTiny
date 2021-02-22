using System.Net;
using System.Windows.Input;

namespace TocTinyWPF.ViewModel
{
    [PropertyChanged.ImplementPropertyChanged]
    internal class LoginViewModel
    {
        public string NickName { get; set; } = "Null";
        public ushort ServerPort { get; set; } = 2020;
        public IPAddress RemoteServer { get; set; } = IPAddress.Loopback;
        public string Password { get; set; } = "114514";
    }
}
