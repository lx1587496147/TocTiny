using CHO.Json;
using Null.Library;
using Null.Library.ConsArgsParser;
using Null.Library.EventedSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TocTiny
{
    internal class Program
    {
        private static string channelName;
        private static readonly List<Socket> clients = new List<Socket>();        // 客户端们
        private static readonly List<Socket> clientToRemove = new List<Socket>();
        private static readonly List<byte[]> lastMessages = new List<byte[]>();
        private static readonly Dictionary<string, (string, Socket)> clientRecord = new Dictionary<string, (string, Socket)>();         // guid为xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        private static readonly int maxSaveCount = 15;
        private static int port, bufferSize, backlog, btimeout, cinterval;
        private static DynamicScanner scanner;
        private static bool nocolor, nocmd;
        private static readonly List<string> AdminGuid = new List<string>();
        private static readonly List<User> UserList = new List<User>();
        private static string USERJSONPATH = "Users.json";
        private static object lockobj=new object();
        private class StartupArgs
        {
            public string PORT = "2020";           // port 
            public string BUFFER = "12345678";      // buffer
            public string BACKLOG = "50";          // backlog
            public string BTIMEOUT = "1000";       // buffer timeout : 缓冲区存活时间 (ms)
            public string CINTERVAL = "1000";      // cleaning interval : 内存清理间隔 (ms)

            public string NAME = "TOC Tiny Chat Room";

            public string USERJSONPATH = "Users.json"; //指定用户JSON路径

            public bool NOCOLOR = false;           // string color : 是否开启控制台的消息文本高亮
            public bool NOCMD = false;
        }
        private static Func<string, bool> SafeWriteHook;
        private static void SafeWriteLine(string content)
        {
            lock (lockobj)
            {
                Task.Run(() => RSafeWriteLine(content)).Wait();
            }
        }

        private static void RSafeWriteLine(string content)
        {
            if (SafeWriteHook != null)
            {
                if (SafeWriteHook.Invoke(content) == false) { return; }
            }
            if (scanner != null && scanner.IsInputting)
            {
                scanner.ClearDisplayBuffer();
                Console.WriteLine(content);
                Console.ForegroundColor = ConsoleColor.Gray;
                scanner.DisplayTo(Console.CursorLeft, Console.CursorTop);
            }
            else
            {
                Console.WriteLine(content);
            }
        }

        private static void DisplayHelpAndEnd()
        {
            SafeWriteLine("TOC Tiny : Server program for TOC Tiny\n\n  TocTiny[-Port port][-Buffer bufferSize][-Backlog backlog][/? | / Help]\n\n    port: Port number to be listened.Default value is 2020.\n    bufferSize: Buffer size for receive data. Default value is 1024576(B)\n    backlog    : Maximum length of the pending connections queue.Default value is 50.\n");
            Environment.Exit(0);
        }

        private static void InitializeUser(string JsonPath)
        {
            if (File.Exists(JsonPath))
            {
                StreamReader streamReader = new StreamReader(JsonPath);
                JsonData jsonData = JsonSerializer.Parse(streamReader.ReadToEnd());
                foreach(JsonData jsonData1 in jsonData)
                {
                    UserList.Add(JsonSerializer.ConvertToInstance<User>(jsonData1));
                }
                streamReader.BaseStream.Close();
                streamReader.Close();
            }
            else
            {
                JsonData jsonData = new JsonData() { DataType = JsonDataType.Array ,content =new List<JsonData>() };
                foreach (User usr in UserList)
                {
                    jsonData.Add(JsonSerializer.Create(usr));
                }
                StreamWriter streamWriter = new StreamWriter(JsonPath, false);
                streamWriter.Write(JsonSerializer.ConvertToText(jsonData));
                streamWriter.Flush();
                streamWriter.BaseStream.Close();
            }
        }
        private static void SaveUser(string JsonPath)
        {
            JsonData jsonData = new JsonData() { DataType = JsonDataType.Array, content = new List<JsonData>() };
            foreach (User usr in UserList)
            {
                jsonData.Add(JsonSerializer.Create(usr));
            }
            StreamWriter streamWriter = new StreamWriter(JsonPath, false);
            streamWriter.Write(JsonSerializer.ConvertToText(jsonData));
            streamWriter.Flush();
            streamWriter.BaseStream.Close();
        }

        private static void Initialize(string[] args)
        {
            ConsArgs consArgs = new ConsArgs(args);
            if (consArgs.Booleans.Contains("?") || consArgs.Booleans.Contains("HELP"))
            {
                DisplayHelpAndEnd();
            }

            scanner = new DynamicScanner()
            {
                PromptText = "\n>>> "
            };

            SafeWriteLine($"Initilizing...");
            StartupArgs startupArgs = consArgs.ToObject<StartupArgs>();

            channelName = startupArgs.NAME;

            if (!uint.TryParse(startupArgs.PORT, out uint uport))
            {
                SafeWriteLine($"Argument out of range, an integer is required. Argument name: 'Port'.");
                Environment.Exit(-1);
            }
            if (!uint.TryParse(startupArgs.BUFFER, out uint ubuffer))
            {
                SafeWriteLine($"Argument out of range, an integer is required. Argument name: 'Buffer'.");
                Environment.Exit(-1);
            }
            if (!uint.TryParse(startupArgs.BACKLOG, out uint ubacklog))
            {
                SafeWriteLine($"Argument out of range, an integer is required. Argument name: 'Backlog'.");
                Environment.Exit(-1);
            }
            if (!uint.TryParse(startupArgs.BTIMEOUT, out uint ubtimeout))
            {
                SafeWriteLine($"Argument out of range, an integer is required. Argument name: 'BTimeout'.");
                Environment.Exit(-1);
            }
            if (!uint.TryParse(startupArgs.CINTERVAL, out uint ucinterval))
            {
                SafeWriteLine($"Argument out of range, an integer is required. Argument name: 'CInterval'.");
                Environment.Exit(-1);
            }

            nocolor = startupArgs.NOCOLOR;
            nocmd = startupArgs.NOCMD;
            USERJSONPATH = startupArgs.USERJSONPATH;

            port = (int)uport;
            bufferSize = (int)ubuffer;
            backlog = (int)ubacklog;
            btimeout = (int)ubtimeout;
            cinterval = (int)ucinterval;

            InitializeUser(USERJSONPATH);

            //new Thread(MemoryCleaningLoop).Start();                     // 开启内存循环清理线程
        }

        private static void Main(string[] args)
        {
            Initialize(args);

            SocketServer server = new SocketServer();
            try
            {
                server.Start((ushort)port, backlog, bufferSize);
                SafeWriteLine($"Server started. Port: {port}, Backlog: {backlog}, Buffer: {bufferSize}(B).");
                server.ClientConnected += Server_ClientConnected;
                server.ClientDisconnected += Server_ClientDisconnected;
                server.RecvedClientMsg += Server_RecvedClientMsg;
                if (nocmd)
                {
                    SafeWriteLine("Server command is unavailable now.");
                    while (server.Running)
                    {
                        Console.ReadKey(true);
                    }
                }
                else
                {
                    SafeWriteLine("Server command is available now. use '/help' for help.");
                    while (server.Running)
                    {
                        string cmd = scanner.ReadLine();
                        DealCommand(cmd);
                    }
                }
            }
            catch
            {
                SafeWriteLine($"Start failed. check if the port {port} is being listened.");
                Environment.Exit(-2);
            }
        }
        private static void DisposePartsBuffer(Socket socket)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            SafeWriteLine($"PartsBuffer Disposed: {socket.RemoteEndPoint}");
        }
        private static void DealCommand(string cmd)
        {
            if (cmd.StartsWith("/"))
            {

            }
            else
            {
                TransPackage msg = new TransPackage()
                {
                    Name = "Server",
                    Content = cmd,
                    ClientGuid = "Server",
                    PackageType = ConstDef.Verification
                };

                BoardcastPackage(msg);
            }
        }                                               // 解释指令
        private static void BoardcastPackage(TransPackage package)
        {
            JsonData jsonData = JsonSerializer.Create(package);
            string jsonText = JsonSerializer.ConvertToText(jsonData);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonText);
            BoardcastData(bytes, bytes.Length);
        }                                // 广播包
        private static bool TrySendData(Socket socket, byte[] data, int size, bool autoRemove = false)
        {
            try
            {
                if (size <= bufferSize)
                {
                    socket.SendTOC(data);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                if (autoRemove)
                {
                    lock (clients)
                    {
                        clients.Remove(socket);
                    }
                }
                return false;
            }
        }    // 尝试发送数据
        private static void BoardcastData(byte[] data, int size)
        {
            foreach (Socket i in clients)
            {
                try
                {
                    i.SendTOC(data);
                }
                catch
                {
                    clientToRemove.Add(i);
                }
            }

            lock (clients)
            {
                foreach (Socket i in clientToRemove)
                {
                    clients.Remove(i);
                    SafeWriteLine($"Removed a disconnected socket which address is {i.RemoteEndPoint}");
                }
            }

            clientToRemove.Clear();
        }                                  // 广播数据
        private static void DrawAttention(string senderName, string senderGuid)
        {
            byte[] attentionData = Encoding.UTF8.GetBytes(
                JsonSerializer.ConvertToText(
                    JsonSerializer.Create(new TransPackage()
                    {
                        Name = senderName,
                        Content = null,
                        ClientGuid = senderGuid,
                        PackageType = ConstDef.DrawAttention
                    })));

            BoardcastData(attentionData, attentionData.Length);
        }                   // 发送吸引注意力
        private static void Server_RecvedClientMsg(object sender, Socket socket, byte[] buffer, int size)
        {
                string recvStr = Encoding.UTF8.GetString(buffer, 0, size);
                JsonData[] recvJsons = JsonSerializer.ParseStream(recvStr);
                foreach (JsonData recvJson in recvJsons)
                {
                    TransPackage recvPackage = JsonSerializer.ConvertToInstance<TransPackage>(recvJson);
                    DealRecvPackage(recvPackage, ref socket, ref buffer, size);
                }
        }        // 当收到客户端消息
        private static void Server_ClientDisconnected(object sender, Socket socket)
        {
            SafeWriteLine($"Removed a disconnected socket which address is {socket.RemoteEndPoint}");

            lock (clients)
            {
                clients.Remove(socket);
            }
        }                              // 当客户端断开
        private static void Server_ClientConnected(object sender,Socket socket)
        {
            SafeWriteLine($"News socket connected, address: {socket.RemoteEndPoint}");

            lock (clients)
            {
                clients.Add(socket);
            }

            new Thread(() =>
            {
                foreach (byte[] msg in lastMessages)
                {
                    TrySendData(socket, msg, msg.Length);
                }
            }).Start();
        }              // 当客户端连接
        private static void AddMessageRecord(byte[] buffer, int size)
        {
            byte[] newRecord = buffer.Take(size).ToArray();
            lastMessages.Add(newRecord);
            if (lastMessages.Count > maxSaveCount)
            {
                lastMessages.RemoveAt(0);
            }
        }                             // 添加消息记录
        private static void DealRecvPackage(TransPackage recvPackage, ref Socket socket, ref byte[] buffer, int size)
        {
            if (recvPackage.PackageType != ConstDef.HeartPackage)
            {
                if (string.IsNullOrWhiteSpace(recvPackage.ClientGuid))
                {
                    if (recvPackage.PackageType == ConstDef.NormalMessage)
                    {
                        string[] args = recvPackage.Content.Split(' ');
                        if (args.Length == 2)
                        {
                            switch (args[0])
                            {
                                case "/login":
                                    recvPackage.PackageType = ConstDef.Login;
                                    recvPackage.Content = args[1];
                                    break;
                                case "/register":
                                    recvPackage.PackageType = ConstDef.Register;
                                    recvPackage.Content = args[1];
                                    break;
                                default:
                                    SendMustLoginInfo(socket);
                                    return;
                            }
                        }
                        else
                        {
                            SendMustLoginInfo(socket);
                            return;
                        }
                    }
                    User user = UserList.Find((u) => u.Name == recvPackage.Name);
                    switch (recvPackage.PackageType)
                    {
                        case ConstDef.Login:
                            if (user != null)
                            {
                                if (user.PasswordHash == recvPackage.Content.GetHashCode())
                                {
                                    socket.SendTOC(
                                        Encoding.UTF8.GetBytes(
                                            JsonSerializer.ConvertToText(
                                                JsonSerializer.Create(new TransPackage()
                                                {
                                                    Name = "Server",
                                                    Content = user.Guid,
                                                    ClientGuid = "Server",
                                                    PackageType = ConstDef.Login
                                                }))));
                                    WelcomeUser(recvPackage, socket);
                                }
                                else
                                {
                                    SendErrorPasswordNotCorrent(socket);
                                }
                            }
                            else
                            {
                                SendErrorUserDoesntExist(socket);
                            }
                            break;
                        case ConstDef.Register:
                            if (user == null)
                            {
                                User newuser = CreateUser(recvPackage);
                                socket.SendTOC(
                                    Encoding.UTF8.GetBytes(
                                        JsonSerializer.ConvertToText(
                                            JsonSerializer.Create(new TransPackage()
                                            {
                                                Name = "Server",
                                                Content = newuser.Guid,
                                                ClientGuid = "Server",
                                                PackageType = ConstDef.Login
                                            }))));
                                WelcomeUser(recvPackage, socket);
                            }
                            else
                            {
                                SendErrorUserExisted(socket);
                            }
                            break;
                        default:
                            SendMustLoginInfo(socket);
                            return;
                    }
                    return;
                }
                switch (recvPackage.PackageType)
                {
                    case ConstDef.NormalMessage:
                        BoardcastData(buffer, size);
                        AddMessageRecord(buffer, size);
                        Console.ForegroundColor = ConsoleColor.Green;
                        SafeWriteLine($"{recvPackage.Name}: {recvPackage.Content}");
                        break;
                    case ConstDef.Verification:
                        clientRecord[recvPackage.ClientGuid] = (recvPackage.Name, socket);
                        BoardcastData(buffer, size);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        SafeWriteLine($"Verification: {recvPackage.Name} - {recvPackage.Content}");
                        break;
                    case ConstDef.ImageMessage:
                        BoardcastData(buffer, size);
                        AddMessageRecord(buffer, size);
                        Console.ForegroundColor = ConsoleColor.Blue;
                        SafeWriteLine($"Image: {recvPackage.Name} - Base string length: {recvPackage.Content.Length}");
                        break;
                    case ConstDef.DrawAttention:
                        DrawAttention(recvPackage.Name, recvPackage.ClientGuid);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        SafeWriteLine($"Draw Attention: Sender: {recvPackage.Name}");
                        break;
                    case ConstDef.ChangeChannelName:
                        socket.SendTOC(
                            Encoding.UTF8.GetBytes(
                                JsonSerializer.ConvertToText(
                                    JsonSerializer.Create(new TransPackage()
                                    {
                                        Name = "Server",
                                        Content = channelName,
                                        ClientGuid = "Server",
                                        PackageType = ConstDef.ChangeChannelName
                                    }))));
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        SafeWriteLine($"Channel Name Req: Sender: {recvPackage.Name}");
                        break;
                    case ConstDef.ReportChannelOnline:
                        socket.SendTOC(
                            Encoding.UTF8.GetBytes(
                                JsonSerializer.ConvertToText(
                                    JsonSerializer.Create(new TransPackage()
                                    {
                                        Name = "Server",
                                        Content = $"Online: {clients.Count}, Your IP Address: {((IPEndPoint)socket.RemoteEndPoint).Address}",
                                        ClientGuid = "Server",
                                        PackageType = ConstDef.ReportChannelOnline
                                    }))));
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        SafeWriteLine($"Online Info Req: Sender: {recvPackage.Name}");
                        break;
                    case ConstDef.AdminCommand:
                        SafeWriteLine($"{recvPackage.Name} try to execute a command on this server.");
                        if (!AdminGuid.Contains(recvPackage.ClientGuid))
                        {
                            socket.SendTOC(
                                Encoding.UTF8.GetBytes(
                                    JsonSerializer.ConvertToText(
                                        JsonSerializer.Create(new TransPackage()
                                        {
                                            Name = "Server",
                                            Content = $"You don't have right to execute command on remote server.",
                                            ClientGuid = "Server",
                                            PackageType = ConstDef.NormalMessage
                                        }))));
                        }
                        else
                        {
                            DealCommand(recvPackage.Content);
                            socket.SendTOC(
                                Encoding.UTF8.GetBytes(
                                    JsonSerializer.ConvertToText(
                                        JsonSerializer.Create(new TransPackage()
                                        {
                                            Name = "Server",
                                            Content = $"Command executed!",
                                            ClientGuid = "Server",
                                            PackageType = ConstDef.NormalMessage
                                        }))));
                            int count = 0;
                            Socket socket1 = socket;
                            bool p(string a)
                            {
                                count += 1;
                                if (count == 2)
                                {
                                    SafeWriteHook -= p;
                                }
                                socket1.Send(
                                Encoding.UTF8.GetBytes(
                                    JsonSerializer.ConvertToText(
                                        JsonSerializer.Create(new TransPackage()
                                        {
                                            Name = "Server",
                                            Content = $"Return:{a}",
                                            ClientGuid = "Server",
                                            PackageType = ConstDef.NormalMessage
                                        }))));
                                return true;
                            }
                            SafeWriteHook += p;
                        }
                        break;
                    case ConstDef.ReportTerminalOutput:
                        SafeWriteLine($"{recvPackage.Name} try to hook terminal on this server.");
                        if (!AdminGuid.Contains(recvPackage.ClientGuid))
                        {
                            socket.SendTOC(
                                Encoding.UTF8.GetBytes(
                                    JsonSerializer.ConvertToText(
                                        JsonSerializer.Create(new TransPackage()
                                        {
                                            Name = "Server",
                                            Content = $"You don't have right to hook terminal on remote server.",
                                            ClientGuid = "Server",
                                            PackageType = ConstDef.NormalMessage
                                        }))));
                        }
                        else
                        {
                            int count = 0;
                            int l = int.Parse(recvPackage.Content);
                            Socket socket1 = socket;
                            bool p(string a)
                            {
                                count += 1;
                                socket1.SendTOC(
                                Encoding.UTF8.GetBytes(
                                    JsonSerializer.ConvertToText(
                                        JsonSerializer.Create(new TransPackage()
                                        {
                                            Name = "Server",
                                            Content = $"Return:{a}",
                                            ClientGuid = "Server",
                                            PackageType = ConstDef.NormalMessage
                                        }))));
                                return true;
                            }
                            SafeWriteHook += p;
                        }
                        break;
                    default:
                        SafeWriteLine($"Recieved data from {socket.RemoteEndPoint}, size: {size}, but cannot be processed.");
                        break;
                }
            }
        }      // 处理消息 (主函数

        private static void SendErrorPasswordNotCorrent(Socket socket)
        {
            socket.SendTOC(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.ConvertToText(
                        JsonSerializer.Create(new TransPackage()
                        {
                            Name = "Server",
                            Content = $"Password error.",
                            ClientGuid = "Server",
                            PackageType = ConstDef.NormalMessage
                        }))));
        }

        private static void SendErrorUserDoesntExist(Socket socket)
        {
            socket.SendTOC(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.ConvertToText(
                        JsonSerializer.Create(new TransPackage()
                        {
                            Name = "Server",
                            Content = $"Error:This user doesn't exist.",
                            ClientGuid = "Server",
                            PackageType = ConstDef.NormalMessage
                        }
                        )
                        )
                    )
                );
        }

        private static User CreateUser(TransPackage recvPackage)
        {
            User newuser = new User
            {
                Name = recvPackage.Name,
                PasswordHash = recvPackage.Content.GetHashCode(),
                Guid = Guid.NewGuid().ToString()
            };
            UserList.Add(newuser);
            SaveUser(USERJSONPATH);
            return newuser;
        }

        private static void WelcomeUser(TransPackage recvPackage, Socket socket)
        {
            socket.SendTOC(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.ConvertToText(
                        JsonSerializer.Create(new TransPackage()
                        {
                            Name = "Server",
                            Content = $"Welcome!{recvPackage.Name}",
                            ClientGuid = "Server",
                            PackageType = ConstDef.NormalMessage
                        }))));
        }

        private static void SendErrorUserExisted(Socket socket)
        {
            socket.SendTOC(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.ConvertToText(
                        JsonSerializer.Create(new TransPackage()
                        {
                            Name = "Server",
                            Content = $"Error:This user name existed.",
                            ClientGuid = "Server",
                            PackageType = ConstDef.NormalMessage
                        }
                        )
                        )
                    )
                );
        }

        private static void SendMustLoginInfo(Socket socket)
        {
            socket.SendTOC(
                    Encoding.UTF8.GetBytes(
                        JsonSerializer.ConvertToText(
                            JsonSerializer.Create(new TransPackage()
                            {
                                Name = "Server",
                                Content = $"You must be logged in to send messages on the remote server.\r\n" +
                                $"If you were using old TocTiny,\r\n" +
                                $"you can try to send \"/login (password here)\"\r\n" +
                                $"or \"/register (password here)\"",
                                ClientGuid = "Server",
                                PackageType = ConstDef.NormalMessage
                            }))));
        }

        private static TransPackage CreateChangeChannelPackage(string channelName)
        {
            return new TransPackage()
            {
                Name = "Server",
                Content = channelName,
                ClientGuid = "Server",
                PackageType = ConstDef.ChangeChannelName
            };
        }
    }
}
