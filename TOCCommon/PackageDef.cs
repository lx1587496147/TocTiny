namespace TocTiny
{
    public static class ConstDef
    {
        public const int NormalMessage = 0b_0000000000000001;
        public const int Verification = 0b_0000000000000010;
        public const int Login = 0b_1101101011011101;
        public const int Register = 0b_1101101011011001;
        public const int ImageMessage = 0b_0000000000000011;
        public const int DrawAttention = 0b_0000000000000100;
        public const int HeartPackage = 0b_1000000000000001;
        public const int AdminCommand = 0b_0101011100101001;
        public const int ReportTerminalOutput = 0b_0011010101000110;
        public const int ChangeChannelName = 0b_1000000000000010;
        public const int ReportChannelOnline = 0b_1000000000000011;
    }
    public class TransPackage
    {
        public string Name;
        public string Content;
        public string ClientGuid;
        public int PackageType;
    }
    public class User
    {
        public string Name;
        public int PasswordHash;
        public string Guid;
    }
}
