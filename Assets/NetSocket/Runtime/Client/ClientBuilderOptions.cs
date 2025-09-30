namespace UFrame.NetSocket
{
    public class ClientBuilderOptions : IBuilderOptions
    {
        public int TcpPort { get; set; }
        public int UdpPort { get; set; }
        public string Ip { get; set; }
        public int UdpPortLocal { get; set; }
        public int PacketSizeBuffer { get; set; }
        public int PacketPartSize { get; set; }

        public ClientBuilderOptions()
        {
            this.PacketSizeBuffer = 3500;
            this.PacketPartSize = 64;
        }
    }
}