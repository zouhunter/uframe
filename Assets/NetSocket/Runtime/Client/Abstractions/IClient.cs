using System;
using System.Net.Sockets;

namespace UFrame.NetSocket
{
    public interface IClient
    {
        EventHandler<Socket> Connected { get; set; }
        EventHandler<Socket> Disconnected { get; set; }
        void SendTcp(ushort opcode, byte[] data);
        void SendTcp(ushort opcode, byte[] data, int offset, int length);
        void SendTcp<T>(ushort opcode, T packet);
        void SendUdp(ushort opcode, byte[] data);
        void SendUdp(ushort opcode, byte[] data, int offset, int length);
        void SendUdp<T>(ushort opcode, T packet);
        void SendUdp(byte[] packet, int length);
        long Ping(int timeout = 10000);
        ConnectResult Connect();
        void Stop();
        void MainThreadRefresh();
    }
}
