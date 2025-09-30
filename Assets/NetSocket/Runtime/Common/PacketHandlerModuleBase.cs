using System;
using System.Collections.Generic;


namespace UFrame.NetSocket
{
	public abstract class PacketHandlerModuleBase : IPacketHandlerModule
	{
		private readonly Dictionary<ushort, Type> _packetHandlers = new Dictionary<ushort, Type>();

		public Dictionary<ushort, Type> GetPacketHandlers()
		{
			return _packetHandlers;
		}

		public void AddPacketHandler<TPacketHandler>(ushort opcode)
		{
			_packetHandlers[opcode] = typeof(TPacketHandler);
		}
	}
}