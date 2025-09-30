using System.Collections.Generic;


namespace UFrame.NetSocket
{
	public class PacketHandlers : IPacketHandlers
	{
		private readonly Dictionary<ushort, IPacketHandler> packetHandlers;

		public PacketHandlers()
		{
			packetHandlers = new Dictionary<ushort, IPacketHandler>();
		}

		public void Add(ushort opcode, IPacketHandler packetHandler)
		{
			packetHandlers.Add(opcode, packetHandler);
		}

		public Dictionary<ushort, IPacketHandler> GetPacketHandlers()
		{
			return packetHandlers;
		}
	}
}