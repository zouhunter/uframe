using System;


namespace UFrame.NetSocket
{
	public class TcpConnectionDisconnectedEventArgs : EventArgs
	{
		public TcpConnectionDisconnectedEventArgs(ITcpConnection connection)
		{
			Connection = connection;
		}

		public ITcpConnection Connection { get; }
	}
}