using System.Net;

namespace UFrame.NetSocket
{
	public class UdpSender : ISender
	{
		private readonly IUdpSocketSender _socketSender;
		private readonly IPacketSerialiser m_packetSerializer;
		private readonly PacketSendPool m_packetSendPool;
		public UdpSender(IPacketSerialiser packetSerializer,IUdpSocketSender socketSender, PacketSendPool packetSendPool)
		{
			_socketSender = socketSender;
			m_packetSerializer = packetSerializer;
			m_packetSendPool = packetSendPool;
		}

		public EndPoint RemoteEndpoint { get; set; }

		public IPEndPoint EndPoint => RemoteEndpoint as IPEndPoint;

        public void Send(ushort opcode, byte[] data)
        {
            var packetData = m_packetSendPool.GetEmptyPacket(data.Length);
			packetData.MakePacket(opcode, data, 0, data.Length);
			_socketSender.SendTo(packetData.buffer, packetData.length, EndPoint);
            m_packetSendPool.SaveBackPacket(packetData);
		}

        public void Send<T>(ushort opcode, T packet)
        {
			var bytes = m_packetSerializer.Serialize<T>(packet);
			Send(opcode, bytes);
		}
    }
}