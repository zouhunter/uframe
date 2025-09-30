using System.Collections.Generic;

namespace UFrame.NetSocket
{
	public interface IPacketContext
	{
		ISender Sender { get; set; }
		ushort Opcode { get; set; }
        public int PacketLength { get; set; }
		byte[] PacketBytes { get; set; }
    }
}