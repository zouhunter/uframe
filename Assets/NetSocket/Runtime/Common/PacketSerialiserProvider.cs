

namespace UFrame.NetSocket
{
	public class PacketSerialiserProvider
	{
		public static IPacketSerialiser PacketSerialiser { get; set; }

		public static IPacketSerialiser Provide()
		{
			return PacketSerialiser;
		}
	}
}