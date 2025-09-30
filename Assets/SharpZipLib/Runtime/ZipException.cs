using System;

namespace UFrame.SharpZipLib
{
	public class ZipException : Exception
	{
		public ZipException()
		{
		}

		public ZipException(string msg)
			: base(msg)
		{
		}
	}
}
