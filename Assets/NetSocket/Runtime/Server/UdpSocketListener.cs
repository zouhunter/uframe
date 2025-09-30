using System.Net;
using System.Net.Sockets;




namespace UFrame.NetSocket
{
	public class UdpSocketListener : IUdpSocketListener
	{
		private readonly IBufferManager bufferManager;
		private readonly ILogger logger;
		private readonly ServerBuilderOptions options;
		private readonly IServerPacketProcessor serverPacketProcessor;
		private readonly ObjectPool<SocketAsyncEventArgs> socketEventArgsPool;
		private IPEndPoint endPoint;
		private Socket listener;

		public UdpSocketListener(ServerBuilderOptions options,
			ILogger logger,
			IServerPacketProcessor serverPacketProcessor,
			IBufferManager bufferManager)
		{
			this.options = options;
			this.logger = logger;
			this.serverPacketProcessor = serverPacketProcessor;
			this.bufferManager = bufferManager;
            socketEventArgsPool = new ObjectPool<SocketAsyncEventArgs>(CreateSocketAsyncFunc);
        }

        public IPEndPoint GetEndPoint()
		{
			return endPoint;
		}

		public Socket GetSocket()
		{
			return listener;
		}

		private SocketAsyncEventArgs CreateSocketAsyncFunc()
        {
            var socketEventArgs = new SocketAsyncEventArgs();
            socketEventArgs.Completed += ProcessReceivedData;
            socketEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, options.UdpPort);
			return socketEventArgs;
        }

		public void Listen()
		{
			listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			endPoint = new IPEndPoint(IPAddress.Loopback, options.UdpPort);
			listener.Bind(endPoint);
			logger.LogDebug($"Starting UDP listener on port {options.UdpPort}.");
			listener.Listen(10000);
			Process();
		}

		private void Process()
		{
			var recieveArgs = socketEventArgsPool.Pop();
			bufferManager.SetBuffer(recieveArgs);

			if (!listener.ReceiveFromAsync(recieveArgs)) ProcessReceivedData(this, recieveArgs);

			StartAccept(recieveArgs);
		}

		private void ProcessReceivedData(object sender, SocketAsyncEventArgs e)
		{
			Process();

			if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
			{
				e.SetBuffer(e.Offset, e.BytesTransferred);
				serverPacketProcessor.ProcessUdp(e);
			}

			socketEventArgsPool.Push(e);
		}

		private void StartAccept(SocketAsyncEventArgs acceptEventArg)
		{
			var willRaiseEvent = listener.AcceptAsync(acceptEventArg);

			if (!willRaiseEvent) ProcessReceivedData(this, acceptEventArg);
		}
	}
}