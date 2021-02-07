using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TocTinyWPF.Model
{
    class SocketTransportPackage
    {
        public static SocketTransportPackage CreateFromSocket(Socket sk)
        {
            byte[] buffer = new byte[8];
            while (sk.Available < 8)
            {
                Thread.Sleep(1);
            }
            sk.Receive(buffer);
            BinaryReader binaryReader = new BinaryReader(new MemoryStream(buffer));
            if (binaryReader.ReadInt32() == 0x544F4332)
            {
                int count = binaryReader.ReadInt32();
                Array.Resize(ref buffer, count);
                sk.Receive(buffer);
                string str = Encoding.UTF8.GetString(buffer);
                SocketTransportPackage package = new SocketTransportPackage();
                package.Load = CHO.Json.JsonSerializer.ConvertToInstance<TransportPackage>(CHO.Json.JsonSerializer.Parse(str));
                return package;
            }
            else
            { return null; }
        }
        /// <summary>
        /// 包负载(应为TransportPackage)
        /// </summary>
        public object Load { get; set; }
        /// <summary>
        /// 包长度
        /// </summary>
        public int PackageLength
        {
            get
            {
                return LoadLength + 8 + ExtraHead.Length;//8是Head+Length的长度
            }
        }
        /// <summary>
        /// 头标识（默认为TOC2）
        /// </summary>
        public int Head { get;} = 0x544F4332;
        /// <summary>
        /// 负载长度
        /// </summary>
        public int LoadLength
        {
            get
            {
                return Encoding.UTF8.GetByteCount(Load.ToString());
            }
        }
        /// <summary>
        /// 额外头数据
        /// </summary>
        public byte[] ExtraHead { get; set; } = new byte[0];
        public void WriteToSocket(Socket sk)
        {
            MemoryStream output = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(output, Encoding.UTF8);
            bw.Write(Head);//写入Head
            bw.Write(LoadLength);//写入负载长度
            bw.Write(Encoding.UTF8.GetBytes(Load.ToString()));//写入负载
            bw.Flush();
            sk.Send(output.ToArray());
        }
    }
}
