using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace UFrame.NetSocket
{
    public abstract class PacketHandlerBase<T> : IPacketHandler
        where T : class
    {
        private Queue<PacketContext> m_packetQueue = new Queue<PacketContext>();

        public void Handle(PacketContext context, bool toMainThread)
        {
            T packet = context.Serializer.Deserialise<T>(context.PacketBytes, 6, context.PacketLength - 6);
            if (toMainThread)
            {
                context.PacketData = packet;
                m_packetQueue.Enqueue(context);
            }
            else
            {
                Process(packet, context);
            }
        }

        public void MainThreadRefresh()
        {
            while (m_packetQueue.Count > 0)
            {
                var packetContext = m_packetQueue.Dequeue();
                Process(packetContext.PacketData as T, packetContext);
                packetContext.Release();
            }
        }

        public abstract void Process(T packet,IPacketContext context);
    }
}