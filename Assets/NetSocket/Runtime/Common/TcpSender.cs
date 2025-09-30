using System.Net;
using System.Net.Sockets;


namespace UFrame.NetSocket
{
	public class TcpSender : ISender
	{
		private readonly IPacketSerialiser m_packetSerialiser;
        private readonly PacketSendPool m_packetSendPool;

        public TcpSender(PacketSendPool packetSendPool ,IPacketSerialiser packetSerialiser)
		{
			m_packetSendPool = packetSendPool;
			m_packetSerialiser = packetSerialiser;
		}

		public Socket Socket { get; set; }

		public IPEndPoint EndPoint => Socket.RemoteEndPoint as IPEndPoint;

        public void Send(ushort opcode, byte[] data)
        {
            var packetData = m_packetSendPool.GetEmptyPacket(data.Length + 6);
            int dataLength = data.Length + 2;
            packetData.MakePacket(opcode,data,0,data.Length);
            Socket.Send(packetData.buffer, 0, packetData.length, SocketFlags.None);
            m_packetSendPool.SaveBackPacket(packetData);
		}

        public void Send<T>(ushort opcode, T packet)
        {
            var packetData = m_packetSerialiser.Serialize<T>(packet);
            Send(opcode, packetData);
        }
    }
}