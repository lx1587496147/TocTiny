using CHO.Json;
using Null.Library;
using Null.Library.ConsArgsParser;
using Null.Library.EventedSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        private static readonly int maxSaveCount = 15;
        private static int bufferSize = 114514, port = 1919, backlog = 810;
        private static DynamicScanner scanner;
        private static bool nocolor, nocmd;
        private static readonly List<string> AdminGuid = new List<string>();
        private static readonly List<User> UserList = new List<User>();
        private static string USERJSONPATH = "Users.json";
        private static bool cuimode = true;
        private static string logpath = "logs";
        private static readonly object lockobj = new object();
        private class StartupArgs
        {
            public string PORT = "2020";           // port 
            public string BUFFER = "114514";      // buffer
            public string BACKLOG = "50";          // backlog

            public string NAME = "TOC Tiny Chat Room";

            public string USERJSONPATH = "Users.json"; //指定用户JSON路径

            public bool NOCOLOR = false;           // string color : 是否开启控制台的消息文本高亮
            public bool NOCMD = false;
        }
        private static bool HasValue(string[] commandline, string argname)
        {
            return commandline.Any((x) => x == argname);
        }
        private static string GetArg(string[] commandline, string argname)
        {
            try
            {
                return (string)commandline.GetValue(
(from n in commandline where n == $"-argname" || n == $"/{argname}" select Array.IndexOf(commandline, n)).FirstOrDefault() + 1);
            }
            catch
            { return ""; }
        }

        private static void DisplayHelpAndEnd()
        {
            PrintLine("TOC Tiny : TOC Tiny的服务器程序\n" +
                "  TocTiny[-Port port][-Buffer bufferSize][-Backlog backlog][/? | / Help]\n" +
                "    port       : 监听端口\n" +
                "    backlog    : 最大连接数默认50\n" +
                "    name       : 服务器的频道名\n" +
                "    logmode    : 指示服务器不应该以CUI图像模式运行\n" +
                "    cuimode    : 指示服务器不应该以log模式运行\n" +
                "    logpath    : 服务器存放日志_目录_\n" +
                "    userath    : 服务器用户数据存放_目录_\n"
                );
            Environment.Exit(0);
        }

        private static void PrintLine(string v,[CallerLineNumber] int LineNumber = 0, [CallerMemberName] string CallMember = "")
        {
                WriteLog(v, LineNumber: LineNumber,Module : "PrintLine",CallMember : CallMember);
        }

        private static void WriteLog(string log,string Module = "TOCTinyServer", [CallerMemberName] string CallMember = "", [CallerLineNumber]int LineNumber = 0)
        {
            string value = $"{DateTime.Now}[{Module}]<{CallMember}>(Line:{LineNumber}){log}";
            if (LogStream != null)
            { LogStream.WriteLineAsync(value); LogStream.Flush(); }
            Console.Out.WriteLineAsync(value);
        }

        private static void Crash(string v, [CallerLineNumber] int LineNumber = 0, [CallerMemberName] string CallMember = "")
        {
            WriteLog(v, LineNumber: LineNumber, CallMember: CallMember,Module:"Crash");
            if (LogStream != null)
                LogStream.Flush();
            Environment.Exit(0);
        }
        private static TextWriter LogStream;

        private static void InitLog()
        {
            PrintLine("开启日志...");
            Directory.CreateDirectory(logpath);
            if (cuimode) LogStream =TextWriter.Synchronized( new StreamWriter(File.Create(Path.Combine(logpath,$"TOCTinyServerLog{DateTime.Now.Ticks}.log")),encoding:Encoding.UTF8,114514));
        }
        
        private static void MakeRTGreatAgaen()
        {
            PrintLine("恭喜您发现了这个彩蛋www:\n" +
                "null家住在下北泽的第810个小镇\n" +
"小镇里有1919个多亲家庭\n" +
"也就是114514个homo\n" +
"null他爸是这个镇的镇长\n" +
"\n" +
"他爸给了他1145141919810m ^ 3的房子\n" +
"butt是1m * 1m * 1145141919810m的房子\n" +
"理论上可以住下11451419198100个人\n" +
"butt因为违章建筑影响了下北泽电视卫星\n" +
"后来被拆成了1919m ^ 3的房子\n" +
"原来null可以和11451419198099个人同时击剑\n" +
"现在null只能和19190个人击剑\n" +
"null很不乐意\n" +
"怎么想都是亏了\n" +
"于是null打开了gayhub\n" +
"和神秘人py了一个地球OL的后门\n" +
"Null弹出了一个UAC弹窗\n" +
"这时一个管理员经过\n" +
"点了取消\n" +
"\n" +
"null说:当老子是空气?\n" +
"继续弹出UAC弹窗\n" +
"管理员不得不点击确认\n" +
"null的诡计得逞了\n" +
"输入了\n" +
"sendcmd 1 OLDataBase delete * from objects\n" +
"\"Done!\"\n" +
"\n" +
"结果忘记加where\n" +
"这时null超越人类极限输入了del /s /q /f C:\\*.*\n" +
"并shutdown - s - t 0\n" +
"地球OL服务器崩溃\n");
        }

        private static bool IsFakePackage(TransPackage package)
        {
            return package.PackageType != 0 && CheckUserGuid(package.ClientGuid, package.Name);
        }

        private static bool CheckUserGuid(string GUID,string name)
        {
            if (GUID == ""||GUID == "Server" || name == "Server") { return false; };
            return UserList.Any((usr) => (usr.Guid == GUID) && (usr.Name == name));
        }
        private static void InitializeUser(string JsonPath)
        {
            if (File.Exists(JsonPath))
            {
                StreamReader streamReader = new StreamReader(JsonPath);
                JsonData jsonData = JsonSerializer.Parse(streamReader.ReadToEnd());
                foreach (JsonData jsonData1 in jsonData)
                {
                    UserList.Add(JsonSerializer.ConvertToInstance<User>(jsonData1));
                }
                streamReader.BaseStream.Close();
                streamReader.Close();
            }
            else
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
            if (HasValue(args,"?") || HasValue(args, "help"))
            {
                DisplayHelpAndEnd();
            }
            PrintLine("TOC Tiny Server v1.114514（TOC协议版本v2）\n" +
                "CHO版权所有\n" +
                "\nTOC启动中...\n");
            try
            {
                channelName = GetArg(args, "channelname");
                nocolor = HasValue(args, "nocolor");
                nocmd = HasValue(args, "nocmd");
                USERJSONPATH = Path.Combine(GetArg(args, "userpath"), "TOCDataBase.DB.Json");
                try { port = int.Parse(GetArg(args, "port")); } catch { }
                try{ bufferSize = int.Parse(GetArg(args, "port"));} catch { }
                try{ backlog = int.Parse(GetArg(args, "port"));}catch { }
            }
            catch (Exception e)
            {
                PrintLine($"初始化错误:可能的原因是初始化使用的值不正确\n" +
                    $"堆栈追踪:\n{e.StackTrace}");
                Environment.Exit(0);
            }
            try
            {
                InitializeUser(USERJSONPATH);
            }
            catch
            {
                PrintLine("用户初始化失败");
            }
            InitLog();
            //new Thread(MemoryCleaningLoop).Start();                     // 开启内存循环清理线程
        }

        private static void Main(string[] args)
        {
            Initialize(args);

            SocketServer server = new SocketServer();
            try
            {
                server.Start((ushort)port, backlog, bufferSize);
                PrintLine($"Server started. Port: {port}, Backlog: {backlog}, Buffer: {bufferSize}(B).");
                server.ClientConnected += Server_ClientConnected;
                server.ClientDisconnected += Server_ClientDisconnected;
                server.RecvedClientMsg += Server_RecvedClientMsg;
                if (cuimode)
                {
                    Console.Clear();
                    CUIModeLoop(server);
                }
                else
                {
                    LogModeLoop(server);
                }
            }
            catch
            {
                PrintLine($"Start failed. check if the port {port} is being listened.");
                Environment.Exit(-2);
            }
        }

        private static void CUIModeLoop(SocketServer server)
        {
            while (server.Running)
            {
                Console.Clear();
                PrintLine("欢迎！这里是TOCTiny的后端管理CUI的咕\n" +
                    "这是一个导航页，你可以通过这一页跳转到其他页面.\n" +
                    "1.服务器总览\n" +
                    "2.用户数据库\n" +
                    "3.TOC配置\n" +
                    "4.终端\n" +
                    "9.杂项以及鸣谢名单\n");
                switch (Console.ReadKey(true).KeyChar)
                {
                    case '1':

                        break;
                    case '2':

                        break;
                    case '3':

                        break;
                    case '4':

                        break;
                    case '9':
                        PrintLine("彳亍欸，TOCTiny服务端现在主要由null+憨包编写，目前已分叉成两个分支.");
                        PrintLine("你要不康康null的故事？(Y/Y)");
                        Console.ReadKey();
                        MakeRTGreatAgaen();
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void LogModeLoop(SocketServer server)
        {
            PrintLine("服务器启动完毕");
            while (server.Running)
            {
                Console.Write("TOCTiny > ");
                string cmd = Console.ReadLine();
                PrintLine($"DealCommand:{cmd}");
                DealCommand(cmd);
            }
        }

        private static void DealCommand(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd)) { return; }
            if (cmd.StartsWith("/"))
            {
                switch (cmd.ToLower().Substring(0, 5))
                {
                    case "/help":

                    case "/cusr":
                        string[] args = cmd.Split(" ");
                        UserList.Add(new User() { Guid = Guid.NewGuid().ToString(), PasswordHash = args[2].GetHashCode(), Name = args[1] });
                        SaveUser(USERJSONPATH);
                        break;
                    default:
                        break;
                }
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
                    PrintLine($"Removed a disconnected socket which address is {i.RemoteEndPoint}");
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
            PrintLine($"Removed a disconnected socket which address is {socket.RemoteEndPoint}");

            lock (clients)
            {
                clients.Remove(socket);
            }
        }                              // 当客户端断开
        private static void Server_ClientConnected(object sender, Socket socket)
        {
            PrintLine($"News socket connected, address: {socket.RemoteEndPoint}");

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
                if (IsFakePackage(recvPackage))
                {
                    DealUnloginUser(recvPackage, socket);
                    return;
                }
                switch (recvPackage.PackageType)
                {
                    case ConstDef.ChangeChannelName:
                        ChangeChannelName(recvPackage, socket);
                        break;
                    case ConstDef.ReportChannelOnline:
                        ReportChannelOnline(recvPackage, socket);
                        break;
                    case ConstDef.AdminCommand:
                        DealAdminCommand(recvPackage, socket);
                        break;
                    case ConstDef.ReportTerminalOutput:
                        ReportTerminalOutput(recvPackage, socket);
                        break;
                    default:
                        DealNormalPackage(recvPackage, buffer, size);
                        break;
                }
            }
        }      // 处理消息 (主函数

        private static void DealNormalPackage(TransPackage recvPackage, byte[] buffer, int size)
        {
            BoardcastData(buffer, size);
            AddMessageRecord(buffer, size);
            ConsoleColor oldcolor = Console.ForegroundColor;
            if (recvPackage.Content.Length > 250)
            {
                if (!nocolor) { Console.ForegroundColor = ConsoleColor.Red; }
                PrintLine($"{recvPackage.Name}: {new string(recvPackage.Content.Take(250).ToArray())}......(Length:{recvPackage.Content.Length})");
                if (recvPackage.PackageType == ConstDef.ImageMessage)
                {
                    try
                    {
                        if (!nocolor)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        PrintLine($"{GetThumbnailChar((Bitmap)Image.FromStream(new MemoryStream(Convert.FromBase64String(recvPackage.Content))))}");
                    }
                    catch
                    {
                        PrintLine("Cann't Get Thumbnail Image");
                    }
                }
            }
            else if (recvPackage.Content.Length > 100)
            {
                if (!nocolor) { Console.ForegroundColor = ConsoleColor.Green; }
                PrintLine($"{recvPackage.Name}: {recvPackage.Content}");
            }
            else
            {
                if (!nocolor) { Console.ForegroundColor = ConsoleColor.Blue; }
                PrintLine($"{recvPackage.Name}: {recvPackage.Content}");
            }
            Console.ForegroundColor = oldcolor;
        }

        private static void ChangeChannelName(TransPackage recvPackage, Socket socket)
        {
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
            PrintLine($"Channel Name Req: Sender: {recvPackage.Name}");
        }

        private static void ReportChannelOnline(TransPackage recvPackage, Socket socket)
        {
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
            PrintLine($"Online Info Req: Sender: {recvPackage.Name}");
        }

        private static void DealAdminCommand(TransPackage recvPackage, Socket socket)
        {
            PrintLine($"{recvPackage.Name} try to execute a command on this server.");
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
                        //SafeWriteHook -= p;
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
                //SafeWriteHook += p;
            }
        }

        private static void ReportTerminalOutput(TransPackage recvPackage, Socket socket)
        {
            PrintLine($"{recvPackage.Name} try to hook terminal on this server.");
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
                //SafeWriteHook += p;
            }
        }

        /// <summary>
        /// 将图片转换为字符画
        /// </summary>
        /// <param name="bitmap">Bitmap类型的对象</param>
        public static string GetThumbnailChar(Bitmap bitmap)
        {
            StringBuilder sb = new StringBuilder();
            string replaceChar = "@*#$%XB0H?OC7>+v=~^:_-'`. ";
            bitmap = (Bitmap)bitmap.GetThumbnailImage((int)(2 * bitmap.Width * (32.0f / bitmap.Height)), 32, null, IntPtr.Zero);
            for (int i = 0; i < bitmap.Height; i += 1)
            {
                for (int j = 0; j < bitmap.Width; j += 1)
                {
                    //获取当前点的Color对象
                    Color c = bitmap.GetPixel(j, i);
                    //计算转化过灰度图之后的rgb值（套用已有的计算公式就行）
                    int rgb = (int)(c.R * .3 + c.G * .59 + c.B * .11);
                    //计算出replaceChar中要替换字符的index
                    //所以根据当前灰度所占总rgb的比例(rgb值最大为255，为了防止超出索引界限所以/256.0)
                    //（肯定是小于1的小数）乘以总共要替换字符的字符数，获取当前灰度程度在字符串中的复杂程度
                    int index = (int)(rgb / 256.0 * replaceChar.Length);
                    //追加进入sb
                    sb.Append(replaceChar[index]);
                }
                //添加换行
                sb.Append("\r\n");
            }
            return sb.ToString();
        }
        private static void DealUnloginUser(TransPackage recvPackage, Socket socket)
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
                            Name = "Server(AutoReplay)",
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
                            Name = "Server(AutoReplay)",
                            Content = $"Error:This user name existed.",
                            ClientGuid = "Server",
                            PackageType = ConstDef.Verification
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
                                Name = "Server(AutoReplay)",
                                Content = $"You must be logged in to send messages on the remote server.",
                                ClientGuid = "Server",
                                PackageType = ConstDef.Verification
                            }))));
        }

    }
}
