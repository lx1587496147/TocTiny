using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TocTinyWPF.Model
{
    class NetworkService
    {
        private Socket socket;
        private Task tsk;
        private bool tskcancel;
        public void Connect(IPAddress ip,int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ip, port);
        }
        public void Start()
        {
            tsk = Task.Factory.StartNew(() => 
            {
                while (!tskcancel)
                {
                    SocketTransportPackage package = SocketTransportPackage.CreateFromSocket(socket);
                    package = package ?? new SocketTransportPackage();
                    PackageReviced.Invoke(this, new Eventarg.PackageRevicedEventArgs() { Package = package });
                }
            });
        }
        public void Send(TransportPackage package)
        {
            SocketTransportPackage package1 = new SocketTransportPackage
            {
                Load = package
            };
            package1.WriteToSocket(socket);
        }
        public event EventHandler<Eventarg.PackageRevicedEventArgs> PackageReviced;
        public void Close()
        {
            tskcancel = true;
            socket.Close();
        }
    }
}
