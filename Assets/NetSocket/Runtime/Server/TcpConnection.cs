
using System.Net.Sockets;

namespace UFrame.NetSocket
{
    public class TcpConnection : ITcpConnection
    {
        public Socket Socket { get; set; }

        public TcpConnection(Socket socket)
        {
            this.Socket = socket;
        }
    }
}
