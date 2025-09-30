namespace UFrame.NetSocket
{
	public class ClientBuilder : BuilderBase<IClientBuilder, IClient, ClientBuilderOptions>, IClientBuilder
	{
		public override IClient Build()
		{
			SetupSharedDependencies();
			serviceCollection.AddSingleton<IClient, Client>();
			serviceCollection.AddSingleton<IClientPacketProcessor, ClientPacketProcessor>();
			serviceCollection.AddSingleton<IUdpSocketSender, UdpClientSender>();
			CollectPackets();
			//var serviceProvider = GetServiceCollection();
			return serviceCollection.GetService(typeof(IClient)) as IClient;
		}

		public T Build<T>()
			where T : IClient
		{
			SetupSharedDependencies();
			serviceCollection.AddSingleton(typeof(T), typeof(Client));
			serviceCollection.AddSingleton<IClientPacketProcessor, ClientPacketProcessor>();
			serviceCollection.AddSingleton<IUdpSocketSender, UdpClientSender>();
			CollectPackets();
			//var serviceProvider = GetServiceCollection();
			return (T)serviceCollection.GetService(typeof(T));
		}

		public IClientBuilder UseIp(string ip)
		{
			if (ip.ToLower() == "localhost") ip = "127.0.0.1";

			options.Ip = ip;
			return this;
		}

		public IClientBuilder UseUdp(int port, int localPort)
		{
			options.UdpPort = port;
			options.UdpPortLocal = localPort;
			return this;
		}
	}
}