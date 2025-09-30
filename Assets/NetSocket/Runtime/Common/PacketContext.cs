using System.Collections.Generic;


namespace UFrame.NetSocket
{
	public class PacketContext : IPacketContext
	{
		public byte[] PacketBytes { get; set; }
		public int PacketLength { get; set; }
		public ushort Opcode { get; set; }
		public ISender Sender { get; set; }
		internal IPacketSerialiser Serializer { get; set; }
		internal System.Action<PacketContext> onRelease { get; set; }
		internal object PacketData { get; set; }

		internal void Release()
        {
			PacketData = null;
			PacketBytes = null;
			Sender = null;
			onRelease?.Invoke(this);
		}
	}
}