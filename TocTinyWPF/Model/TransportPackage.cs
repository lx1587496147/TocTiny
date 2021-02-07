using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TocTinyWPF.Model
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
    class TransportPackage
    {
        public string Name;
        public string Content;
        public string ClientGuid;
        public int PackageType;
        public override string ToString()
        {
            return CHO.Json.JsonSerializer.ConvertToText(CHO.Json.JsonSerializer.Create(this));
        }
    }
}
