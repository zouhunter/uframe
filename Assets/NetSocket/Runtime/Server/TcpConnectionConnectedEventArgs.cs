using System;


namespace UFrame.NetSocket
{
	public class TcpConnectionConnectedEventArgs : EventArgs
	{
		public TcpConnectionConnectedEventArgs(ITcpConnection connection)
		{
			Connection = connection;
		}

		public ITcpConnection Connection { get; }
	}
}