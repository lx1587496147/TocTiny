using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;

namespace TocTinyWPF.ViewModel
{
    class LoginViewModel
    {
        public string NickName { get; set; } = "Null";
        public ushort ServerPort { get; set; } = 2020;
        public IPAddress RemoteServer { get; set; } = IPAddress.Loopback;
        public string Password { get; set; } = "114514";
        public ICommand ConnectCommand { get; } = new ActionCommand(() => { });
    }
}
