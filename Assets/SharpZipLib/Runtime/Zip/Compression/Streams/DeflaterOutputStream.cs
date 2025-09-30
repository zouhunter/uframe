using System;
using System.IO;
using UFrame.SharpZipLib.Checksums;

namespace UFrame.SharpZipLib.Zip.Compression.Streams
{
	public class DeflaterOutputStream : Stream
	{
		protected byte[] buf;

		protected Deflater def;

		protected Stream baseOutputStream;

		private string password = null;

		private uint[] keys = null;

		public override bool CanRead => baseOutputStream.CanRead;

		public override bool CanSeek => false;

		public override bool CanWrite => baseOutputStream.CanWrite;

		public override long Length => baseOutputStream.Length;

		public override long Position
		{
			get
			{
				return baseOutputStream.Position;
			}
			set
			{
				baseOutputStream.Position = value;
			}
		}

		public string Password
		{
			get
			{
				return password;
			}
			set
			{
				password = value;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek not supported");
		}

		public override void SetLength(long val)
		{
			baseOutputStream.SetLength(val);
		}

		public override int ReadByte()
		{
			return baseOutputStream.ReadByte();
		}

		public override int Read(byte[] b, int off, int len)
		{
			return baseOutputStream.Read(b, off, len);
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException("Asynch read not currently supported");
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException("Asynch write not currently supported");
		}

		protected void Deflate()
		{
			while (!def.IsNeedingInput)
			{
				int num = def.Deflate(buf, 0, buf.Length);
				if (num <= 0)
				{
					break;
				}
				baseOutputStream.Write(buf, 0, num);
			}
			if (!def.IsNeedingInput)
			{
				throw new ApplicationException("Can't deflate all input?");
			}
		}

		public DeflaterOutputStream(Stream baseOutputStream)
			: this(baseOutputStream, new Deflater(), 512)
		{
		}

		public DeflaterOutputStream(Stream baseOutputStream, Deflater defl)
			: this(baseOutputStream, defl, 512)
		{
		}

		public DeflaterOutputStream(Stream baseOutputStream, Deflater defl, int bufsize)
		{
			this.baseOutputStream = baseOutputStream;
			if (bufsize <= 0)
			{
				throw new InvalidOperationException("bufsize <= 0");
			}
			buf = new byte[bufsize];
			def = defl;
		}

		public override void Flush()
		{
			def.Flush();
			Deflate();
			baseOutputStream.Flush();
		}

		public virtual void Finish()
		{
			def.Finish();
			while (!def.IsFinished)
			{
				int num = def.Deflate(buf, 0, buf.Length);
				if (num <= 0)
				{
					break;
				}
				if (Password != null)
				{
					EncryptBlock(buf, 0, num);
				}
				baseOutputStream.Write(buf, 0, num);
			}
			if (!def.IsFinished)
			{
				throw new ApplicationException("Can't deflate all input?");
			}
			baseOutputStream.Flush();
		}

		public override void Close()
		{
			Finish();
			baseOutputStream.Close();
		}

		public override void WriteByte(byte bval)
		{
			Write(new byte[1] { bval }, 0, 1);
		}

		public override void Write(byte[] buf, int off, int len)
		{
			def.SetInput(buf, off, len);
			Deflate();
		}

		protected byte EncryptByte()
		{
			uint num = (keys[2] & 0xFFFFu) | 2u;
			return (byte)(num * (num ^ 1) >> 8);
		}

		protected void EncryptBlock(byte[] buf, int off, int len)
		{
			for (int i = off; i < off + len; i++)
			{
				byte ch = buf[i];
				byte[] array;
				byte[] array2 = (array = buf);
				int num = i;
				nint num2 = num;
				array2[num] = (byte)(array[num2] ^ EncryptByte());
				UpdateKeys(ch);
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
