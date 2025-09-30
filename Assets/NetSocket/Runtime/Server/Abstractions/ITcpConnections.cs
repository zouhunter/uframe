using System.Collections.Generic;
using System.Net.Sockets;

namespace UFrame.NetSocket
{
    public interface ITcpConnections
    {
        List<ITcpConnection> GetConnections();

        ITcpConnection Add(Socket connection);
        void Remove(ITcpConnection connection);
    }
}
