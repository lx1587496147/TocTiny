using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TocTinyWPF.Package
{
    public class SizePackage<T> : IBinaryPackage where T : TextPackage
    {
        public static SizePackage<T> CreateFromStream(Stream sk)
        {
            byte[] buffer = new byte[8];
            sk.Read(buffer, 0, 8);
            if (Bytes2Int(buffer) == 0x544F4332)
            {
                int count = Bytes2Int(buffer.Skip(4).ToArray());
                Array.Resize(ref buffer, count);
                sk.Read(buffer, 0, buffer.Length);
                string str = Encoding.UTF8.GetString(buffer);
                SizePackage<T> package = new SizePackage<T>
                {
                    Load = System.Text.Json.JsonSerializer.Deserialize<T>(str)
                };
                return package;
            }
            else
            { return null; }
        }
        /// <summary>
        /// 包负载(应为TransportPackage)
        /// </summary>
        public T Load { get; set; }
        /// <summary>
        /// 头标识（默认为TOC2）
        /// </summary>
        public int Head { get; } = 0x544F4332;
        /// <summary>
        /// 负载长度
        /// </summary>
        public int LoadLength => Load.ToBytes().Length;
        private static byte[] Int2Bytes(int num1) => new byte[] { (byte)(num1 & 255), (byte)((num1 >> 8) & 255), (byte)((num1 >> 16) & 255), (byte)((num1 >> 24) & 255) };
        private static int Bytes2Int(byte[] vs) => vs[0] | (vs[1] << 8) | (vs[2] << 16) | (vs[3] << 24);
        public byte[] ToBytes() => Int2Bytes(Head).Concat(Int2Bytes(LoadLength)).Concat(Load.ToBytes()).ToArray();
    }
}
