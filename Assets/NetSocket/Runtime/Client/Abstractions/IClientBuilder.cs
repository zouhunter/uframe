

namespace UFrame.NetSocket
{
	public interface IClientBuilder : IBuilder<IClientBuilder, IClient>
	{
		T Build<T>() where T : IClient;
		//Ip
		IClientBuilder UseIp(string ip);
		//Udp
		IClientBuilder UseUdp(int port, int localPort);
	}
}