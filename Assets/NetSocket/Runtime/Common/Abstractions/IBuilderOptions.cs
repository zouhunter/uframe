

namespace UFrame.NetSocket
{
	public interface IBuilderOptions
	{
		int PacketSizeBuffer { get; set; }
		int TcpPort { get; set; }
		int UdpPort { get; set; }
	}
}