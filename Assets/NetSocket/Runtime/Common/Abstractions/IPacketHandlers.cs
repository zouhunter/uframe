using System.Collections.Generic;

namespace UFrame.NetSocket
{
	public interface IPacketHandlers
	{
		void Add(ushort opcode, IPacketHandler packetHandler);
		Dictionary<ushort, IPacketHandler> GetPacketHandlers();
	}
}