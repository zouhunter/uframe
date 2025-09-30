using System.Net.Sockets;

namespace UFrame.NetSocket
{
    public interface ISocketListener
    {
        Socket GetSocket();
        void Listen();
    }
}
