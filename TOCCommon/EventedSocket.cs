using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Null.Library.EventedSocket
{
    public delegate void SocketConnectedHandler(object sender, Socket socket);
    public delegate void SocketDisconnectedHandler(object sender, Socket socket);
    public delegate void SocketRecvMsgHandler(object sender, Socket socket, byte[] buffer, int size);
    public class SocketServer
    {
        private Socket server;                                               // 用来接受连接的套接字
        private readonly List<(Task, Socket)> workingSockets = new List<(Task, Socket)>();
        private int bufferSize;
        /// <summary>
        /// 基础Socket
        /// </summary>
        [Obsolete("为了兼容性请不要使用Socket，请使用Send发送信息", true)] public Socket Socket => server;
        /// <summary>
        /// 是否在运行
        /// </summary>
        public bool Running => server.IsBound;
        /// <summary>
        /// 连接数
        /// </summary>
        public int ConnectedCount => workingSockets.Count;
        /// <summary>
        /// 获得监听Socket的任务
        /// </summary>
        public Task ListenTask { get; protected set; }
        /// <summary>
        /// 启动监听
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="backlog">连接列队限制</param>
        /// <param name="bufferSize">缓冲区大小</param>
        public void Start(ushort port, int backlog, int bufferSize)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.bufferSize = bufferSize;
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(backlog);
            ListenTask = Task.Factory.StartNew(AcceptAction);
        }
        /// <summary>
        /// 停止监听
        /// </summary>
        public void Stop()
        {
            server.Close();
        }

        public event SocketConnectedHandler ClientConnected;
        public event SocketDisconnectedHandler ClientDisconnected;
        public event SocketRecvMsgHandler RecvedClientMsg;

        /// <summary>
        /// 接受连接操作
        /// </summary>
        /// <param name="ar">Sokcet连接异步结果</param>
        private void AcceptAction()
        {
            while (true)
            {
                Socket client = server.Accept();
                ClientConnected.Invoke(this, client);
                workingSockets.Add((Task.Factory.StartNew(() => ReceiveAction(client)), client));
            }
        }

        /// <summary>
        /// 接受数据操作
        /// </summary>
        /// <param name="ar">Sokcet连接异步结果</param>
        private void ReceiveAction(Socket socket)
        {
            Socket client = socket;
            try
            {
                while (true)
                {
                    byte[] vs = new byte[8];
                    client.Receive(vs);
                    BinaryReader br = new BinaryReader(new MemoryStream(vs));
                    int head = br.ReadInt32();
                    int len = br.ReadInt32();
                    br.Dispose();
                    if (head == 0x544F4332)
                    {
                        byte[] vs1 = new byte[0];
                        Array.Resize(ref vs1, len);
                        client.Receive(vs1);
                        try //控制异常范围
                        {
                            RecvedClientMsg.Invoke(this, client, vs1, len);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        throw new Exception(); //抛出异常实现什么都不做
                    }
                }
            }
            catch
            {
                ClientDisconnected.Invoke(this, client);
                client.Dispose();
            }
        }

    }
    [Obsolete("将会使用TocTinyService重构")]
    public class SocketClient
    {
        /// <summary>
        /// 基础Socket
        /// </summary>
        private Socket server;
        /// <summary>
        /// 获得连接到服务器的Sokcet
        /// </summary>
        [Obsolete("为了兼容性请不要使用SocketToServer，请使用Send发送信息", true)] public Socket SocketToServer => server;
        /// <summary>
        /// 获得监听Socket的任务
        /// </summary>
        public Task ListenTask { get; protected set; }
        /// <summary>
        /// 连接到目标主机
        /// </summary>
        /// <param name="address">主机地址</param>
        /// <param name="bufferSize">缓冲区大小</param>
        public void ConnectTo(IPEndPoint address, int bufferSize)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Connect(address);
            ListenTask = Task.Factory.StartNew(() => ReceiveAction());
        }
        public object Tag { get; set; }
        public event SocketRecvMsgHandler ReceivedMsg;
        public event SocketDisconnectedHandler Disconnected;

        /// <summary>
        /// 接受操作
        /// </summary>
        /// <param name="ar">Sokcet连接异步结果</param>
        private void ReceiveAction()
        {
            try
            {
                while (true)
                {
                    byte[] vs = new byte[8];
                    server.Receive(vs);
                    BinaryReader br = new BinaryReader(new MemoryStream(vs));
                    int head = br.ReadInt32();
                    int len = br.ReadInt32();
                    br.Dispose();
                    if (head == 0x544F4332)
                    {
                        byte[] vs1 = new byte[0];
                        Array.Resize(ref vs1, len);
                        server.Receive(vs1);
                        try //控制异常范围
                        {
                            ReceivedMsg.Invoke(this, server, vs1, len);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        throw new Exception(); //抛出异常实现什么都不做
                    }
                }
            }
            catch
            {
                Disconnected.Invoke(this, server);
                server.Dispose();
            }
        }
        public int Send(byte[] buffer)
        {
            return server.SendTOC(buffer);
        }
        public int SendDirect(byte[] buffer)
        {
            return server.Send(buffer);
        }
    }
    public class StateObject
    {
        /// <summary>
        /// 工作Socket
        /// </summary>
        public Socket workSocket;
    }
}
public static class SocketExt
{
    public static int SendTOC(this Socket socket, byte[] buffer)
    {
        return SendTOC(socket, buffer, buffer.Length, SocketFlags.None);
    }
    public static int SendTOC(this Socket socket, byte[] buffer, int size, SocketFlags socketFlags)
    {
        byte[] nbuffer = new byte[0] { };
        int length = size;
        Array.Resize(ref nbuffer, length + 8);
        BinaryWriter bw = new BinaryWriter(new MemoryStream(nbuffer));
        bw.Write(0x544F4332);//"TOC2"
        bw.Write(length);
        bw.Write(buffer);
        bw.Dispose();
        return socket.Send(nbuffer);
    }
}