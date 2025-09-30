namespace UFrame.NetSocket
{
	public class ServerBuilderOptions : IBuilderOptions
	{
		public ServerBuilderOptions()
		{
			TcpMaxConnections = 100;
			PacketSizeBuffer = 5000;
			PacketPartSize = 64;
		}

		public int TcpMaxConnections { get; set; }
		public int PacketSizeBuffer { get; set; }
		public int PacketPartSize { get; set; }
		public int TcpPort { get; set; }
		public int UdpPort { get; set; }
	}
}