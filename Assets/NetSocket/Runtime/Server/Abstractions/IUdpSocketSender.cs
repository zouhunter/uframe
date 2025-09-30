using System.Net;

namespace UFrame.NetSocket
{
    public interface IUdpSocketSender
    {
        void SendTo(byte[] packetBytes,int length, IPEndPoint endpoint);
    }
}
