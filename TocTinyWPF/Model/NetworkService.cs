using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TOCPackage = TocTinyWPF.Package.SizePackage<TocTinyWPF.Package.TOCPackage>;

namespace TocTinyWPF.Model
{
    internal class NetworkService
    {
        public NetworkService()
        {
            NetworkFailed = (s, e) => {  };
        }
        public event EventHandler NetworkFailed;
        private TcpClient socket;
        private Stream nwkstm;
        private bool tskcancel;
        public void Connect(IPAddress ip, int port)
        {
            try
            {
                socket = new TcpClient();
                socket.Connect(ip, port);
                nwkstm =Stream.Synchronized( socket.GetStream());
            }
            catch
            {
                OnNetworkFailed();
            }
        }
        private void OnNetworkFailed()
        {
            NetworkFailed.Invoke(this, new EventArgs());
        }
        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (!tskcancel)
                {
                    try
                    {
                        TOCPackage package = TOCPackage.CreateFromStream(nwkstm);
                        if (package == null) continue;
                        PackageReviced.Invoke(this, package);
                    }
                    catch
                    {
                        OnNetworkFailed();
                        break;
                    }
                }
            });
        }
        public void Send(Package.TOCPackage package)
        {
            try
            {
                TOCPackage package1 = new TOCPackage
                {
                    Load = package
                };
                byte[] vs = package1.ToBytes();
                nwkstm.Write(vs,0,vs.Length);
            }
            catch
            {
                OnNetworkFailed();
            }
        }
        public event EventHandler<TOCPackage> PackageReviced;
        public void Close()
        {
            tskcancel = true;
            socket.Close();
        }
    }
}
