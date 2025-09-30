using System;
using System.IO;
using UFrame.SharpZipLib.Checksums;

namespace UFrame.SharpZipLib.Zip.Compression.Streams
{
	public class InflaterInputStream : Stream
	{
		protected Inflater inf;

		protected byte[] buf;

		protected int len;

		private byte[] onebytebuffer = new byte[1];

		protected Stream baseInputStream;

		protected long csize;

		protected byte[] cryptbuffer = null;

		private uint[] keys = null;

		public override bool CanRead => baseInputStream.CanRead;

		public override bool CanSeek => false;

		public override bool CanWrite => baseInputStream.CanWrite;

		public override long Length => len;

		public override long Position
		{
			get
			{
				return baseInputStream.Position;
			}
			set
			{
				baseInputStream.Position = value;
			}
		}

		public virtual int Available
		{
			get
			{
				if (!inf.IsFinished)
				{
					return 1;
				}
				return 0;
			}
		}

		public override void Flush()
		{
			baseInputStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek not supported");
		}

		public override void SetLength(long val)
		{
			baseInputStream.SetLength(val);
		}

		public override void Write(byte[] array, int offset, int count)
		{
			baseInputStream.Write(array, offset, count);
		}

		public override void WriteByte(byte val)
		{
			baseInputStream.WriteByte(val);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException("Asynch write not currently supported");
		}

		public InflaterInputStream(Stream baseInputStream)
			: this(baseInputStream, new Inflater(), 4096)
		{
		}

		public InflaterInputStream(Stream baseInputStream, Inflater inf)
			: this(baseInputStream, inf, 4096)
		{
		}

		public InflaterInputStream(Stream baseInputStream, Inflater inf, int size)
		{
			this.baseInputStream = baseInputStream;
			this.inf = inf;
			try
			{
				len = (int)baseInputStream.Length;
			}
			catch (Exception)
			{
				len = 0;
			}
			if (size <= 0)
			{
				throw new ArgumentOutOfRangeException("size <= 0");
			}
			buf = new byte[size];
		}
		public override void Close()
		{
			baseInputStream.Close();
		}

		protected void Fill()
		{
			len = baseInputStream.Read(buf, 0, buf.Length);
			if (cryptbuffer != null)
			{
				DecryptBlock(buf, 0, Math.Min((int)(csize - inf.TotalIn), buf.Length));
			}
			if (len <= 0)
			{
				throw new ApplicationException("Deflated stream ends early.");
			}
			inf.SetInput(buf, 0, len);
		}

		public override int ReadByte()
		{
			int num = Read(onebytebuffer, 0, 1);
			if (num > 0)
			{
				return onebytebuffer[0] & 0xFF;
			}
			return -1;
		}

		public override int Read(byte[] b, int off, int len)
		{
			while (true)
			{
				int num;
				try
				{
					num = inf.Inflate(b, off, len);
				}
				catch (Exception ex)
				{
					throw new ZipException(ex.ToString());
				}
				if (num > 0)
				{
					return num;
				}
				if (inf.IsNeedingDictionary)
				{
					throw new ZipException("Need a dictionary");
				}
				if (inf.IsFinished)
				{
					return 0;
				}
				if (!inf.IsNeedingInput)
				{
					break;
				}
				Fill();
			}
			throw new InvalidOperationException("Don't know what to do");
		}

		public long Skip(long n)
		{
			if (n < 0)
			{
				throw new ArgumentOutOfRangeException("n");
			}
			int num = 2048;
			if (n < num)
			{
				num = (int)n;
			}
			byte[] array = new byte[num];
			return baseInputStream.Read(array, 0, array.Length);
		}

		protected byte DecryptByte()
		{
			uint num = (keys[2] & 0xFFFFu) | 2u;
			return (byte)(num * (num ^ 1) >> 8);
		}

		protected void DecryptBlock(byte[] buf, int off, int len)
		{
			for (int i = off; i < off + len; i++)
			{
				byte[] array;
				byte[] array2 = (array = buf);
				int num = i;
				nint num2 = num;
				array2[num] = (byte)(array[num2] ^ DecryptByte());
				UpdateKeys(buf[i]);
			}
		}

		protected void InitializePassword(string password)
		{
			keys = new uint[3] { 305419896u, 591751049u, 878082192u };
			for (int i = 0; i < password.Length; i++)
			{
				UpdateKeys((byte)password[i]);
			}
		}

		protected void UpdateKeys(byte ch)
		{
			keys[0] = Crc32.ComputeCrc32(keys[0], ch);
			keys[1] = keys[1] + (byte)keys[0];
			keys[1] = keys[1] * 134775813 + 1;
			keys[2] = Crc32.ComputeCrc32(keys[2], (byte)(keys[1] >> 24));
		}
	}
}
