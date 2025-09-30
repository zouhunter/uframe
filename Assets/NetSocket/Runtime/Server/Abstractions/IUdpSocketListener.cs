using System.Net;

namespace UFrame.NetSocket
{
    public interface IUdpSocketListener : ISocketListener
    {
        IPEndPoint GetEndPoint();
    }
}
