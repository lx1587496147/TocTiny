# 欢迎来到TOC Tiny的世界
Toc Tiny是一个复杂，凌乱，开源的项目<br/>
<br/>
# 包结构
我们取一个《简单》的心跳包看看：<br/>
2COT<0号字符><0号字符<0号字符><0号字符><br/>
2COT是头标识，占4个字节.<br/>
<0号字符><0号字符<0号字符><0号字符>表示包大小，在这里是0，所以都是0号字符<br/>
<br/>
# 传输方式
连接无状态传输<br/>

# 包枚举

        NormalMessage = 0b_0000000000000001<br/>
        Verification = 0b_0000000000000010<br/>
        Login = 0b_1101101011011101<br/>
        Register = 0b_1101101011011001<br/>
        ImageMessage = 0b_0000000000000011<br/>
        DrawAttention = 0b_0000000000000100<br/>
        HeartPackage = 0b_1000000000000001<br/>
        AdminCommand = 0b_0101011100101001<br/>
        ReportTerminalOutput = 0b_0011010101000110<br/>
        ChangeChannelName = 0b_1000000000000010<br/>
        ReportChannelOnline = 0b_1000000000000011<br/>


# 下一个版本TODO
1.表情系统（客户端）<br/>
2.文件传输（客户端和服务端）<br/>
3.Ban用户（服务端）<br/>
4.CUI界面（服务端）<br/>
5.加密通讯（客户端和服务端）

# 加密交流过程（TODO）
客户端：
1.打开连接，发送Hello包（包括密钥id（随机），预期时间戳）
4.等待服务器的KeyExchange包
5.发送Login包

服务端：
2.接受连接，等待Hello包（预期时间戳必须与服务器时间保持一致）
3.发送对应的KeyExchange包
6.等待Login包，然后返回Login包（GUID