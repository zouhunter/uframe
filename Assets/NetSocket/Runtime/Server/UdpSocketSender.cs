using System.Net;
using System.Net.Sockets;

namespace UFrame.NetSocket
{
	public class UdpSocketSender : IUdpSocketSender
	{
		private readonly IPacketSerialiser _packetSerialiser;
		private readonly IUdpSocketListener _udpSocketListener;

		public UdpSocketSender(IUdpSocketListener udpSocketListener, IPacketSerialiser packetSerialiser)
		{
			_udpSocketListener = udpSocketListener;
			_packetSerialiser = packetSerialiser;
		}

		public void Broadcast(byte[] packetBytes,int length)
		{
			var socket = _udpSocketListener.GetSocket();
			socket.Send(packetBytes,length, SocketFlags.Broadcast);
		}

		public void SendTo(byte[] packetBytes, int length, IPEndPoint endpoint)
		{
			var socket = _udpSocketListener.GetSocket();
			socket.SendTo(packetBytes,length, SocketFlags.None, endpoint);
		}
	}
}