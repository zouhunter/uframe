using System.Collections.Generic;
using System.Net.Sockets;

namespace UFrame.NetSocket
{
    public class TcpConnections : ITcpConnections
    {
        private readonly List<ITcpConnection> connections;
        private readonly ObjectPool<ITcpConnection> objectPool;

        public TcpConnections(ServerBuilderOptions options)
        {
            this.objectPool = new ObjectPool<ITcpConnection>(()=> new TcpConnection(null));
            this.connections = new List<ITcpConnection>();
        }

        public ITcpConnection Add(Socket socket)
        {
            var connection = this.objectPool.Pop();
            connection.Socket = socket;

            lock (this.connections)
            {
                this.connections.Add(connection);
            }

            return connection;
        }

        public List<ITcpConnection> GetConnections()
        {
            lock (this.connections)
            {
                return this.connections;
            }
        }

        public void Remove(ITcpConnection connection)
        {
            lock (this.connections)
            {
                this.connections.Remove(connection);
            }

            connection.Socket = null;
            this.objectPool.Push(connection);
        }
    }
}