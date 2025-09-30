using System.Net.Sockets;


namespace UFrame.NetSocket
{
	public interface IClientPacketProcessor
	{
		void Process(Socket socket);
		void Process(UdpReceiveResult data);
		void SetUdpSocketSender(IUdpSocketSender socketSender);
		void MainThreadRefresh();
	}
}