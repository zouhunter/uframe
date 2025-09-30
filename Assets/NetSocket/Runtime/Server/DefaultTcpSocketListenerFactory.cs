namespace UFrame.NetSocket
{
	public class DefaultTcpSocketListenerFactory : ITcpSocketListenerFactory
	{
		private readonly IBufferManager bufferManager;
		private readonly ITcpConnections connections;
		private readonly ILogger logger;
		private readonly ServerBuilderOptions options;
		private readonly IServerPacketProcessor serverPacketProcessor;

		public DefaultTcpSocketListenerFactory(IServerPacketProcessor serverPacketProcessor,
			IBufferManager bufferManager,
			ILogger logger,
			ITcpConnections connections,
			ServerBuilderOptions options)
		{
			this.serverPacketProcessor = serverPacketProcessor;
			this.bufferManager = bufferManager;
			this.logger = logger;
			this.connections = connections;
			this.options = options;
		}

		public ITcpSocketListener Create()
		{
			return new TcpSocketListener(options,
				serverPacketProcessor,
				bufferManager,
				logger,
				connections);
		}
	}
}