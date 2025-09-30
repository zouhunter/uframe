namespace UFrame.NetSocket
{
    public interface IServerInformation
    {
        bool IsRunning { get; set; }

        int InvalidTcpPackets { get; set; }
        int ProcessedTcpPackets { get; set; }

        int InvalidUdpPackets { get; set; }
        int ProcessedUdpPackets { get; set; }
    }
}
