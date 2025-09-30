using System.Net;

namespace UFrame.NetSocket
{
	public interface ISender
	{
		IPEndPoint EndPoint { get; }
		void Send<T>(ushort opcode,T packet);
        void Send(ushort opcode, byte[] data);
	}
}