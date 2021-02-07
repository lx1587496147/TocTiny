using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TocTiny.Model
{
    class SocketTransportPackage
    {
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
        public void WriteToStream(Stream stm)
        {
            BinaryWriter bw = new BinaryWriter(stm,Encoding.UTF8);
            bw.Write(Head);//写入Head
            bw.Write(LoadLength);//写入负载长度
            bw.Write(ExtraHead);//写入额外数据
            bw.Write(Load.ToString());//写入负载
            bw.Flush();
            bw.Dispose();
        }
    }
}
