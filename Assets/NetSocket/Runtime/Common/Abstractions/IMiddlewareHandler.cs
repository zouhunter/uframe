using System;
using System.Threading.Tasks;

namespace UFrame.NetSocket
{
	public interface IMiddlewareHandler
	{
		Task Process(IPacketContext context, Action nextMiddleware);
	}
}