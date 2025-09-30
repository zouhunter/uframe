using System.Net.Sockets;

namespace UFrame.NetSocket
{
    public interface IBufferManager
    {
        void FreeBuffer(SocketAsyncEventArgs args);
        void InitBuffer();
        bool SetBuffer(SocketAsyncEventArgs args);
    }
}
