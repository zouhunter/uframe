using System;

namespace UFrame.NetSocket
{
    public interface ITcpSocketListener : ISocketListener
    {
        EventHandler<TcpConnectionConnectedEventArgs> ClientConnected { get; set; }
        EventHandler<TcpConnectionDisconnectedEventArgs> ClientDisconnected { get; set; }
    }
}
