﻿
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UFrame.NetSocket
{
    public interface IServerPacketProcessor
    {
        void SetUdpSender(IUdpSocketSender sender);
        Task ProcessFromBuffer(ISender sender, byte[] buffer, int offset = 0, int length = 0, bool isTcp = true);
        void ProcessTcp(SocketAsyncEventArgs socketEvent);
        void ProcessUdp(SocketAsyncEventArgs socketEvent);
        void ProcessUdpFromBuffer(EndPoint sender, byte[] buffer, int offset = 0, int length = 0);
    }
}
