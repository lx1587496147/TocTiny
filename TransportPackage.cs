using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TocTiny.Model
{
    enum PackageType
    {
        NormalMessage = 0b_0000000000000001,
        Verification = 0b_0000000000000010,
        Login = 0b_1101101011011101,
        Register = 0b_1101101011011001,
        ImageMessage = 0b_0000000000000011,
        DrawAttention = 0b_0000000000000100,
        HeartPackage = 0b_1000000000000001,
        AdminCommand = 0b_0101011100101001,
        ReportTerminalOutput = 0b_0011010101000110,
        ChangeChannelName = 0b_1000000000000010,
        ReportChannelOnline = 0b_1000000000000011
    }
    struct TransportPackage
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string ClientGuid { get; set; }
        public PackageType PackageType { get; set; }
        public override string ToString()
        {
            return $"{{" +
                $"  \"Name\":\"{Name}\"," +
                $"  \"Content\":\"{Content}\"," +
                $"  \"ClientGuid\":\"{ClientGuid}\"," +
                $"  \"PackageType\":\"{(int)PackageType}" +
                $"}}";
        }
    }
}
