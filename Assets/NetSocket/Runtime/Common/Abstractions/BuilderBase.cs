using System;
using System.Collections.Generic;

namespace UFrame.NetSocket
{
	public abstract class BuilderBase<TBuilder, TResult, TBuilderOptions> : IBuilder<TBuilder, TResult>
		where TBuilder : class, IBuilder<TBuilder, TResult> where TBuilderOptions : class, IBuilderOptions
	{
		private IConfiguration configuration;

		//private Action<ILoggingBuilder> loggingBuilder;

		//Modules
		protected PacketHandlerModule module;
		protected List<IPacketHandlerModule> modules;

		//Builder Options
		protected TBuilderOptions options;

		//Service Collection
		protected IServiceCollection serviceCollection;
		private Action<IServiceCollection> serviceCollectionFactory;
		//protected Func<IServiceProvider> serviceProviderFactory;

		public BuilderBase()
		{
			serviceCollection = new ServiceCollection();
			options = Activator.CreateInstance<TBuilderOptions>();
			modules = new List<IPacketHandlerModule>();
			module = new PacketHandlerModule();
			modules.Add(module);
		}

		public abstract TResult Build();

		//public TBuilder ConfigureLogging(Action<ILoggingBuilder> loggingBuilder)
		//{
		//	this.loggingBuilder = loggingBuilder;
		//	return this as TBuilder;
		//}

		public IServiceCollection GetServiceCollection()
		{
			return serviceCollection;
		}

		public void CollectPackets()
		{
			var serviceProvider = serviceCollection;
            PacketSerialiserProvider.PacketSerialiser = serviceProvider.GetService<IPacketSerialiser>();
			if(PacketSerialiserProvider.PacketSerialiser == null)
				throw new Exception("No PacketSerialiser  has been configured for UFrame.NetSocket");
			var packetHandlers = serviceProvider.GetService<IPacketHandlers>();
			foreach (var packetHandlerModule in modules)
			foreach (var packetHandler in packetHandlerModule.GetPacketHandlers())
				packetHandlers.Add(packetHandler.Key,
					(IPacketHandler) serviceProvider.GetService(packetHandler.Value));

			//if (!PacketSerialiserProvider.PacketSerialiser.CanReadOpcode && packetHandlers.GetPacketHandlers()
			//	    .Count > 1)
			//	throw new Exception(
			//		"A PacketSerialiser which cannot identify a packet can only support up to one packet type");
		}


		public TBuilder RegisterPacketHandler<TPacketHandler>(ushort opcode) where TPacketHandler : IPacketHandler
		{
			module.AddPacketHandler<TPacketHandler>(opcode);
			return this as TBuilder;
		}

		public TBuilder RegisterPacketHandlerModule(IPacketHandlerModule packetHandlerModule)
		{
			modules.Add(packetHandlerModule);
			return this as TBuilder;
		}

		public TBuilder RegisterPacketHandlerModule<T>() where T : IPacketHandlerModule,new()
		{
			modules.Add(new T());
			return this as TBuilder;
		}

		public TBuilder RegisterTypes(Action<IServiceCollection> serviceCollection)
		{
			serviceCollectionFactory = serviceCollection;
			return this as TBuilder;
		}

		public TBuilder SetPacketBufferSize(int size)
		{
			options.PacketSizeBuffer = size;
			return this as TBuilder;
		}

		public TBuilder SetServiceCollection(IServiceCollection serviceCollection/*  Func<IServiceCollection> serviceProviderFactory = null*/)
		{
			this.serviceCollection = serviceCollection;
			//this.serviceProviderFactory = serviceProviderFactory;
			return this as TBuilder;
		}

		//public TBuilder UseConfiguration(IConfiguration configuration)
		//{
		//	this.configuration = configuration;
		//	serviceCollection.AddSingleton(configuration);
		//	return this as TBuilder;
		//}

		//public TBuilder UseConfiguration<T>(IConfiguration configuration)
		//	where T : class
		//{
		//	this.configuration = configuration;
		//	serviceCollection.AddSingleton(configuration);
		//	serviceCollection.Configure<T>(configuration);
		//	return this as TBuilder;
		//}

		public TBuilder UseTcp(int port)
		{
			options.TcpPort = port;
			return this as TBuilder;
		}

		public TBuilder UseUdp(int port)
		{
			options.UdpPort = port;
			return this as TBuilder;
		}

		public TBuilder UseLog<T>() where T:class,ILogger
        {
			GetServiceCollection().AddSingleton<ILogger,T>();
			return this as TBuilder;
		}

        public TBuilder UsePacketSerialiser<T>() where T : class, IPacketSerialiser
		{
			GetServiceCollection().AddSingleton<IPacketSerialiser, T>();
			return this as TBuilder;
        }

        protected void SetupSharedDependencies()
		{
			foreach (var packetHandlerModule in modules)
			foreach (var packetHandler in packetHandlerModule.GetPacketHandlers())
				serviceCollection.AddSingleton(packetHandler.Value);

			serviceCollection.AddSingleton(options);
			serviceCollection.AddSingleton<IPacketHandlers, PacketHandlers>();

			//if (loggingBuilder == null) loggingBuilder = loggerBuilderFactory => { };

			//serviceCollection.AddLogging(loggingBuilder);
			serviceCollectionFactory?.Invoke(serviceCollection);
		}
	}
}