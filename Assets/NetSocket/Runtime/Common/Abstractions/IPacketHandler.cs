using System.Threading.Tasks;

namespace UFrame.NetSocket
{
    public interface IPacketHandler
    {
        void Handle(PacketContext packetContext,bool toMainThread);
        void MainThreadRefresh();
    }
}
