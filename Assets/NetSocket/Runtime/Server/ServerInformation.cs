

namespace UFrame.NetSocket
{
    public class ServerInformation : IServerInformation
    {
        public bool IsRunning { get; set; }

        public int InvalidTcpPackets { get; set; }
        public int ProcessedTcpPackets { get; set; }

        public int ProcessedUdpPackets { get; set; }
        public int InvalidUdpPackets { get; set; }
    }
}
