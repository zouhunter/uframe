using System;
using System.Collections.Generic;

namespace UFrame.NetSocket
{
	public interface IPacketHandlerModule
	{
		Dictionary<ushort, Type> GetPacketHandlers();
	}
}