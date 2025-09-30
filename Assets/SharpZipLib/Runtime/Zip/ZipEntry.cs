using System;

namespace UFrame.SharpZipLib.Zip
{
	public class ZipEntry : ICloneable
	{
		private static int KNOWN_SIZE = 1;

		private static int KNOWN_CSIZE = 2;

		private static int KNOWN_CRC = 4;

		private static int KNOWN_TIME = 8;

		private string name;

		private uint size;

		private ushort version;

		private uint compressedSize;

		private uint crc;

		private uint dosTime;

		private ushort known = 0;

		private CompressionMethod method = CompressionMethod.Deflated;

		private byte[] extra = null;

		private string comment = null;

		private bool isCrypted;

		private int zipFileIndex = -1;

		private int flags;

		private int offset;

		public bool IsEncrypted
		{
			get
			{
				return (flags & 1) != 0;
			}
			set
			{
				if (value)
				{
					flags |= 1;
				}
				else
				{
					flags &= -2;
				}
			}
		}

		public int ZipFileIndex
		{
			get
			{
				return zipFileIndex;
			}
			set
			{
				zipFileIndex = value;
			}
		}

		public int Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
			}
		}

		public int Flags
		{
			get
			{
				return flags;
			}
			set
			{
				flags = value;
			}
		}

		public int Version
		{
			get
			{
				return version;
			}
			set
			{
				version = (ushort)value;
			}
		}

		public long DosTime
		{
			get
			{
				if ((known & KNOWN_TIME) == 0)
				{
					return 0L;
				}
				return dosTime;
			}
			set
			{
				dosTime = (uint)value;
				known |= (ushort)KNOWN_TIME;
			}
		}

		public DateTime DateTime
		{
			get
			{
				uint second = 2 * (dosTime & 0x1F);
				uint minute = (dosTime >> 5) & 0x3Fu;
				uint hour = (dosTime >> 11) & 0x1Fu;
				uint day = (dosTime >> 16) & 0x1Fu;
				uint month = (dosTime >> 21) & 0xFu;
				uint year = ((dosTime >> 25) & 0x7F) + 1980;
				return new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second);
			}
			set
			{
				DosTime = (uint)((((value.Year - 1980) & 0x7F) << 25) | (value.Month << 21) | (value.Day << 16) | (value.Hour << 11) | (value.Minute << 5) | (int)((uint)value.Second >> 1));
			}
		}

		public string Name => name;

		public long Size
		{
			get
			{
				if ((known & KNOWN_SIZE) == 0)
				{
					return -1L;
				}
				return size;
			}
			set
			{
				if ((value & -4294967296L) != 0)
				{
					throw new ArgumentOutOfRangeException("size");
				}
				size = (uint)value;
				known |= (ushort)KNOWN_SIZE;
			}
		}

		public long CompressedSize
		{
			get
			{
				if ((known & KNOWN_CSIZE) == 0)
				{
					return -1L;
				}
				return compressedSize;
			}
			set
			{
				if ((value & -4294967296L) != 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				compressedSize = (uint)value;
				known |= (ushort)KNOWN_CSIZE;
			}
		}

		public long Crc
		{
			get
			{
				if ((known & KNOWN_CRC) == 0)
				{
					return -1L;
				}
				return (long)crc & 0xFFFFFFFFL;
			}
			set
			{
				if ((crc & -4294967296L) != 0)
				{
					throw new Exception();
				}
				crc = (uint)value;
				known |= (ushort)KNOWN_CRC;
			}
		}

		public CompressionMethod CompressionMethod
		{
			get
			{
				return method;
			}
			set
			{
				method = value;
			}
		}

		public byte[] ExtraData
		{
			get
			{
				return extra;
			}
			set
			{
				if (value == null)
				{
					extra = null;
					return;
				}
				if (value.Length > 65535)
				{
					throw new ArgumentOutOfRangeException();
				}
				extra = value;
				try
				{
					int num2;
					for (int i = 0; i < extra.Length; i += num2)
					{
						int num = (extra[i++] & 0xFF) | ((extra[i++] & 0xFF) << 8);
						num2 = (extra[i++] & 0xFF) | ((extra[i++] & 0xFF) << 8);
						if (num == 21589)
						{
							int num3 = extra[i];
							if (((uint)num3 & (true ? 1u : 0u)) != 0)
							{
								int seconds = (extra[i + 1] & 0xFF) | ((extra[i + 2] & 0xFF) << 8) | ((extra[i + 3] & 0xFF) << 16) | ((extra[i + 4] & 0xFF) << 24);
								DateTime = (new DateTime(1970, 1, 1, 0, 0, 0) + new TimeSpan(0, 0, 0, seconds, 0)).ToLocalTime();
								known |= (ushort)KNOWN_TIME;
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}

		public string Comment
		{
			get
			{
				return comment;
			}
			set
			{
				if (value.Length > 65535)
				{
					throw new ArgumentOutOfRangeException();
				}
				comment = value;
			}
		}

		public bool IsDirectory
		{
			get
			{
				int length = name.Length;
				if (length > 0)
				{
					return name[length - 1] == '/';
				}
				return false;
			}
		}

		public bool IsCrypted
		{
			get
			{
				return isCrypted;
			}
			set
			{
				isCrypted = value;
			}
		}

		public ZipEntry(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			DateTime = DateTime.Now;
			this.name = name;
		}

		public ZipEntry(ZipEntry e)
		{
			name = e.name;
			known = e.known;
			size = e.size;
			compressedSize = e.compressedSize;
			crc = e.crc;
			dosTime = e.dosTime;
			method = e.method;
			extra = e.extra;
			comment = e.comment;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public override string ToString()
		{
			return name;
		}
	}
}
