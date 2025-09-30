using System.Net;

namespace UFrame.NetSocket
{
	public class UdpClientSender : IUdpSocketSender
	{
		private readonly IClient _client;

		public UdpClientSender(IClient client, IClientPacketProcessor clientPacketProcessor)
		{
			_client = client;
			clientPacketProcessor.SetUdpSocketSender(this);
		}

		public void SendTo(byte[] packetBytes,int length, IPEndPoint endpoint)
		{
			_client.SendUdp(packetBytes,length);
		}
	}
}