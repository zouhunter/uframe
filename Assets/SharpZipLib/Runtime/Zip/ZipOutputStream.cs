using System;
using System.Collections;
using System.IO;
using UFrame.SharpZipLib.Checksums;
using UFrame.SharpZipLib.Zip.Compression;
using UFrame.SharpZipLib.Zip.Compression.Streams;

namespace UFrame.SharpZipLib.Zip
{
	public class ZipOutputStream : DeflaterOutputStream
	{
		private const int ZIP_STORED_VERSION = 10;

		private const int ZIP_DEFLATED_VERSION = 20;

		public const int STORED = 0;

		public const int DEFLATED = 8;

		private ArrayList entries = new ArrayList();

		private Crc32 crc = new Crc32();

		private ZipEntry curEntry = null;

		private CompressionMethod curMethod;

		private int size;

		private int offset = 0;

		private byte[] zipComment = new byte[0];

		private int defaultMethod = 8;

		private bool shouldWriteBack = false;

		private long seekPos = -1L;

		public bool CanPatchEntries => baseOutputStream.CanSeek;

		public ZipOutputStream(Stream baseOutputStream)
			: base(baseOutputStream, new Deflater(Deflater.DEFAULT_COMPRESSION, nowrap: true))
		{
		}

		public void SetComment(string comment)
		{
			byte[] array = ZipConstants.ConvertToArray(comment);
			if (array.Length > 65535)
			{
				throw new ArgumentException("Comment too long.");
			}
			zipComment = array;
		}

		public void SetMethod(int method)
		{
			if (method != 0 && method != 8)
			{
				throw new ArgumentException("Method not supported.");
			}
			defaultMethod = method;
		}

		public void SetLevel(int level)
		{
			def.SetLevel(level);
		}

		private void WriteLeShort(int value)
		{
			baseOutputStream.WriteByte((byte)value);
			baseOutputStream.WriteByte((byte)(value >> 8));
		}

		private void WriteLeInt(int value)
		{
			WriteLeShort(value);
			WriteLeShort(value >> 16);
		}

		private void WriteLeLong(long value)
		{
			WriteLeInt((int)value);
			WriteLeInt((int)(value >> 32));
		}

		public void PutNextEntry(ZipEntry entry)
		{
			if (entries == null)
			{
				throw new InvalidOperationException("ZipOutputStream was finished");
			}
			CompressionMethod compressionMethod = entry.CompressionMethod;
			int num = 0;
			entry.IsCrypted = base.Password != null;
			switch (compressionMethod)
			{
			case CompressionMethod.Stored:
				if (entry.CompressedSize >= 0)
				{
					if (entry.Size < 0)
					{
						entry.Size = entry.CompressedSize;
					}
					else if (entry.Size != entry.CompressedSize)
					{
						throw new ZipException("Method STORED, but compressed size != size");
					}
				}
				else
				{
					entry.CompressedSize = entry.Size;
				}
				if (entry.IsCrypted)
				{
					entry.CompressedSize += 12L;
				}
				if (entry.Size < 0)
				{
					throw new ZipException("Method STORED, but size not set");
				}
				if (entry.Crc < 0)
				{
					throw new ZipException("Method STORED, but crc not set");
				}
				break;
			case CompressionMethod.Deflated:
				if (entry.CompressedSize < 0 || entry.Size < 0 || entry.Crc < 0)
				{
					num |= 8;
				}
				break;
			}
			if (curEntry != null)
			{
				CloseEntry();
			}
			if (entry.IsCrypted)
			{
				num |= 1;
			}
			entry.Flags = num;
			entry.Offset = offset;
			entry.CompressionMethod = compressionMethod;
			curMethod = compressionMethod;
			WriteLeInt(67324752);
			WriteLeShort((compressionMethod == CompressionMethod.Stored) ? 10 : 20);
			if ((num & 8) == 0)
			{
				WriteLeShort(num);
				WriteLeShort((byte)compressionMethod);
				WriteLeInt((int)entry.DosTime);
				WriteLeInt((int)entry.Crc);
				WriteLeInt((int)entry.CompressedSize);
				WriteLeInt((int)entry.Size);
			}
			else
			{
				if (baseOutputStream.CanSeek)
				{
					shouldWriteBack = true;
					WriteLeShort((short)(num & -9));
				}
				else
				{
					shouldWriteBack = false;
					WriteLeShort(num);
				}
				WriteLeShort((byte)compressionMethod);
				WriteLeInt((int)entry.DosTime);
				if (baseOutputStream.CanSeek)
				{
					seekPos = baseOutputStream.Position;
				}
				WriteLeInt(0);
				WriteLeInt(0);
				WriteLeInt(0);
			}
			byte[] array = ZipConstants.ConvertToArray(entry.Name);
			if (array.Length > 65535)
			{
				throw new ZipException("Name too long.");
			}
			byte[] array2 = entry.ExtraData;
			if (array2 == null)
			{
				array2 = new byte[0];
			}
			if (array2.Length > 65535)
			{
				throw new ZipException("Extra data too long.");
			}
			WriteLeShort(array.Length);
			WriteLeShort(array2.Length);
			baseOutputStream.Write(array, 0, array.Length);
			baseOutputStream.Write(array2, 0, array2.Length);
			if (base.Password != null)
			{
				InitializePassword(base.Password);
				byte[] array3 = new byte[12];
				Random random = new Random();
				for (int i = 0; i < array3.Length; i++)
				{
					array3[i] = (byte)random.Next();
				}
				EncryptBlock(array3, 0, array3.Length);
				baseOutputStream.Write(array3, 0, array3.Length);
			}
			offset += 30 + array.Length + array2.Length;
			curEntry = entry;
			crc.Reset();
			if (compressionMethod == CompressionMethod.Deflated)
			{
				def.Reset();
			}
			size = 0;
		}

		public void CloseEntry()
		{
			if (curEntry == null)
			{
				throw new InvalidOperationException("No open entry");
			}
			if (curMethod == CompressionMethod.Deflated)
			{
				base.Finish();
			}
			int num = ((curMethod == CompressionMethod.Deflated) ? def.TotalOut : size);
			if (curEntry.Size < 0)
			{
				curEntry.Size = size;
			}
			else if (curEntry.Size != size)
			{
				throw new ZipException("size was " + size + ", but I expected " + curEntry.Size);
			}
			if (curEntry.IsCrypted)
			{
				num += 12;
			}
			if (curEntry.CompressedSize < 0)
			{
				curEntry.CompressedSize = num;
			}
			else if (curEntry.CompressedSize != num)
			{
				throw new ZipException("compressed size was " + num + ", but I expected " + curEntry.CompressedSize);
			}
			if (curEntry.Crc < 0)
			{
				curEntry.Crc = crc.Value;
			}
			else if (curEntry.Crc != crc.Value)
			{
				throw new ZipException("crc was " + crc.Value + ", but I expected " + curEntry.Crc);
			}
			offset += num;
			if (curMethod == CompressionMethod.Deflated && ((uint)curEntry.Flags & 8u) != 0)
			{
				if (shouldWriteBack)
				{
					curEntry.Flags &= -9;
					long position = baseOutputStream.Position;
					baseOutputStream.Seek(seekPos, SeekOrigin.Begin);
					WriteLeInt((int)curEntry.Crc);
					WriteLeInt((int)curEntry.CompressedSize);
					WriteLeInt((int)curEntry.Size);
					baseOutputStream.Seek(position, SeekOrigin.Begin);
					shouldWriteBack = false;
				}
				else
				{
					WriteLeInt(134695760);
					WriteLeInt((int)curEntry.Crc);
					WriteLeInt((int)curEntry.CompressedSize);
					WriteLeInt((int)curEntry.Size);
					offset += 16;
				}
			}
			entries.Add(curEntry);
			curEntry = null;
		}

		public override void Write(byte[] b, int off, int len)
		{
			if (curEntry == null)
			{
				throw new InvalidOperationException("No open entry.");
			}
			switch (curMethod)
			{
			case CompressionMethod.Deflated:
				base.Write(b, off, len);
				break;
			case CompressionMethod.Stored:
				if (base.Password != null)
				{
					byte[] array = new byte[len];
					Array.Copy(b, off, array, 0, len);
					EncryptBlock(array, 0, len);
					baseOutputStream.Write(array, off, len);
				}
				else
				{
					baseOutputStream.Write(b, off, len);
				}
				break;
			}
			crc.Update(b, off, len);
			size += len;
		}

		public override void Finish()
		{
			if (entries == null)
			{
				return;
			}
			if (curEntry != null)
			{
				CloseEntry();
			}
			int num = 0;
			int num2 = 0;
			foreach (ZipEntry entry in entries)
			{
				CompressionMethod compressionMethod = entry.CompressionMethod;
				WriteLeInt(33639248);
				WriteLeShort((compressionMethod == CompressionMethod.Stored) ? 10 : 20);
				WriteLeShort((compressionMethod == CompressionMethod.Stored) ? 10 : 20);
				WriteLeShort(entry.Flags);
				WriteLeShort((short)compressionMethod);
				WriteLeInt((int)entry.DosTime);
				WriteLeInt((int)entry.Crc);
				WriteLeInt((int)entry.CompressedSize);
				WriteLeInt((int)entry.Size);
				byte[] array = ZipConstants.ConvertToArray(entry.Name);
				if (array.Length > 65535)
				{
					throw new ZipException("Name too long.");
				}
				byte[] array2 = entry.ExtraData;
				if (array2 == null)
				{
					array2 = new byte[0];
				}
				string comment = entry.Comment;
				byte[] array3 = ((comment != null) ? ZipConstants.ConvertToArray(comment) : new byte[0]);
				if (array3.Length > 65535)
				{
					throw new ZipException("Comment too long.");
				}
				WriteLeShort(array.Length);
				WriteLeShort(array2.Length);
				WriteLeShort(array3.Length);
				WriteLeShort(0);
				WriteLeShort(0);
				if (entry.IsDirectory)
				{
					WriteLeInt(16);
				}
				else
				{
					WriteLeInt(0);
				}
				WriteLeInt(entry.Offset);
				baseOutputStream.Write(array, 0, array.Length);
				baseOutputStream.Write(array2, 0, array2.Length);
				baseOutputStream.Write(array3, 0, array3.Length);
				num++;
				num2 += 46 + array.Length + array2.Length + array3.Length;
			}
			WriteLeInt(101010256);
			WriteLeShort(0);
			WriteLeShort(0);
			WriteLeShort(num);
			WriteLeShort(num);
			WriteLeInt(num2);
			WriteLeInt(offset);
			WriteLeShort(zipComment.Length);
			baseOutputStream.Write(zipComment, 0, zipComment.Length);
			baseOutputStream.Flush();
			entries = null;
		}
	}
}
