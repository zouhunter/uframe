using System;
using System.Collections;
using System.IO;
using UFrame.SharpZipLib.Zip.Compression;
using UFrame.SharpZipLib.Zip.Compression.Streams;

namespace UFrame.SharpZipLib.Zip
{
	public class ZipFile : IEnumerable
	{
		private class ZipEntryEnumeration : IEnumerator
		{
			private ZipEntry[] array;

			private int ptr = -1;

			public object Current => array[ptr];

			public ZipEntryEnumeration(ZipEntry[] arr)
			{
				array = arr;
			}

			public void Reset()
			{
				ptr = -1;
			}

			public bool MoveNext()
			{
				return ++ptr < array.Length;
			}
		}

		private class PartialInputStream : InflaterInputStream
		{
			private Stream baseStream;

			private long filepos;

			private long end;

			public override int Available
			{
				get
				{
					long num = end - filepos;
					if (num > int.MaxValue)
					{
						return int.MaxValue;
					}
					return (int)num;
				}
			}

			public PartialInputStream(Stream baseStream, long start, long len)
				: base(baseStream)
			{
				this.baseStream = baseStream;
				filepos = start;
				end = start + len;
			}

			public override int ReadByte()
			{
				if (filepos == end)
				{
					return -1;
				}
				lock (baseStream)
				{
					baseStream.Seek(filepos++, SeekOrigin.Begin);
					return baseStream.ReadByte();
				}
			}

			public override int Read(byte[] b, int off, int len)
			{
				if (len > end - filepos)
				{
					len = (int)(end - filepos);
					if (len == 0)
					{
						return 0;
					}
				}
				lock (baseStream)
				{
					baseStream.Seek(filepos, SeekOrigin.Begin);
					int num = baseStream.Read(b, off, len);
					if (num > 0)
					{
						filepos += len;
					}
					return num;
				}
			}

			public long SkipBytes(long amount)
			{
				if (amount < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (amount > end - filepos)
				{
					amount = end - filepos;
				}
				filepos += amount;
				return amount;
			}
		}

		private string name;

		private string comment;

		private Stream baseStream;

		private ZipEntry[] entries;

		public string ZipFileComment => comment;

		public string Name => name;

		public int Size
		{
			get
			{
				try
				{
					return entries.Length;
				}
				catch (Exception)
				{
					throw new InvalidOperationException("ZipFile has closed");
				}
			}
		}

		public ZipFile(string name)
			: this(File.OpenRead(name))
		{
		}

		public ZipFile(FileStream file)
		{
			baseStream = file;
			name = file.Name;
			ReadEntries();
		}

		public ZipFile(Stream baseStream)
		{
			this.baseStream = baseStream;
			name = null;
			ReadEntries();
		}

		private int ReadLeShort()
		{
			return baseStream.ReadByte() | (baseStream.ReadByte() << 8);
		}

		private int ReadLeInt()
		{
			return ReadLeShort() | (ReadLeShort() << 16);
		}

		private void ReadEntries()
		{
			long num = baseStream.Length - 22;
			do
			{
				if (num < 0)
				{
					throw new ZipException("central directory not found, probably not a zip file");
				}
				baseStream.Seek(num--, SeekOrigin.Begin);
			}
			while (ReadLeInt() != 101010256);
			long position = baseStream.Position;
			baseStream.Position += 6L;
			if (baseStream.Position - position != 6)
			{
				throw new EndOfStreamException();
			}
			int num2 = ReadLeShort();
			position = baseStream.Position;
			baseStream.Position += 4L;
			if (baseStream.Position - position != 4)
			{
				throw new EndOfStreamException();
			}
			int num3 = ReadLeInt();
			int num4 = ReadLeShort();
			byte[] array = new byte[num4];
			baseStream.Read(array, 0, array.Length);
			comment = ZipConstants.ConvertToString(array);
			entries = new ZipEntry[num2];
			baseStream.Seek(num3, SeekOrigin.Begin);
			for (int i = 0; i < num2; i++)
			{
				if (ReadLeInt() != 33639248)
				{
					throw new ZipException("Wrong Central Directory signature");
				}
				position = baseStream.Position;
				baseStream.Position += 6L;
				if (baseStream.Position - position != 6)
				{
					throw new EndOfStreamException();
				}
				int compressionMethod = ReadLeShort();
				int num5 = ReadLeInt();
				int num6 = ReadLeInt();
				int num7 = ReadLeInt();
				int num8 = ReadLeInt();
				int num9 = ReadLeShort();
				int num10 = ReadLeShort();
				int num11 = ReadLeShort();
				position = baseStream.Position;
				baseStream.Position += 8L;
				if (baseStream.Position - position != 8)
				{
					throw new EndOfStreamException();
				}
				int offset = ReadLeInt();
				byte[] array2 = new byte[Math.Max(num9, num11)];
				baseStream.Read(array2, 0, num9);
				string text = ZipConstants.ConvertToString(array2);
				ZipEntry zipEntry = new ZipEntry(text);
				zipEntry.CompressionMethod = (CompressionMethod)compressionMethod;
				zipEntry.Crc = num6 & 0xFFFFFFFFu;
				zipEntry.Size = num8 & 0xFFFFFFFFu;
				zipEntry.CompressedSize = num7 & 0xFFFFFFFFu;
				zipEntry.DosTime = (uint)num5;
				if (num10 > 0)
				{
					byte[] array3 = new byte[num10];
					baseStream.Read(array3, 0, num10);
					zipEntry.ExtraData = array3;
				}
				if (num11 > 0)
				{
					baseStream.Read(array2, 0, num11);
					zipEntry.Comment = ZipConstants.ConvertToString(array2);
				}
				zipEntry.ZipFileIndex = i;
				zipEntry.Offset = offset;
				entries[i] = zipEntry;
			}
		}

		public void Close()
		{
			entries = null;
			lock (baseStream)
			{
				baseStream.Close();
			}
		}

		public IEnumerator GetEnumerator()
		{
			if (entries == null)
			{
				throw new InvalidOperationException("ZipFile has closed");
			}
			return new ZipEntryEnumeration(entries);
		}

		private int GetEntryIndex(string name)
		{
			for (int i = 0; i < entries.Length; i++)
			{
				if (name.Equals(entries[i].Name))
				{
					return i;
				}
			}
			return -1;
		}

		public ZipEntry GetEntry(string name)
		{
			if (entries == null)
			{
				throw new InvalidOperationException("ZipFile has closed");
			}
			int entryIndex = GetEntryIndex(name);
			if (entryIndex < 0)
			{
				return null;
			}
			return (ZipEntry)entries[entryIndex].Clone();
		}

		private long CheckLocalHeader(ZipEntry entry)
		{
			lock (baseStream)
			{
				baseStream.Seek(entry.Offset, SeekOrigin.Begin);
				if (ReadLeInt() != 67324752)
				{
					throw new ZipException("Wrong Local header signature");
				}
				long position = baseStream.Position;
				baseStream.Position += 4L;
				if (baseStream.Position - position != 4)
				{
					throw new EndOfStreamException();
				}
				if (entry.CompressionMethod != (CompressionMethod)ReadLeShort())
				{
					throw new ZipException("Compression method mismatch");
				}
				position = baseStream.Position;
				baseStream.Position += 16L;
				if (baseStream.Position - position != 16)
				{
					throw new EndOfStreamException();
				}
				if (entry.Name.Length != ReadLeShort())
				{
					throw new ZipException("file name length mismatch");
				}
				int num = entry.Name.Length + ReadLeShort();
				return entry.Offset + 30 + num;
			}
		}

		public Stream GetInputStream(ZipEntry entry)
		{
			if (entries == null)
			{
				throw new InvalidOperationException("ZipFile has closed");
			}
			int num = entry.ZipFileIndex;
			if (num < 0 || num >= entries.Length || entries[num].Name != entry.Name)
			{
				num = GetEntryIndex(entry.Name);
				if (num < 0)
				{
					throw new IndexOutOfRangeException();
				}
			}
			long start = CheckLocalHeader(entries[num]);
			CompressionMethod compressionMethod = entries[num].CompressionMethod;
			Stream stream = new PartialInputStream(baseStream, start, entries[num].CompressedSize);
			return compressionMethod switch
			{
				CompressionMethod.Stored => stream, 
				CompressionMethod.Deflated => new InflaterInputStream(stream, new Inflater(nowrap: true)), 
				_ => throw new ZipException("Unknown compression method " + compressionMethod), 
			};
		}
	}
}
