using System.Net.Sockets;

namespace UFrame.NetSocket
{
    public interface ITcpConnection
    {
        Socket Socket { get; set; }
    }
}
