using System;
using UFrame.SharpZipLib.Zip.Compression.Streams;

namespace UFrame.SharpZipLib.Zip.Compression
{
	public class InflaterHuffmanTree
	{
		private static int MAX_BITLEN;

		private short[] tree;

		public static InflaterHuffmanTree defLitLenTree;

		public static InflaterHuffmanTree defDistTree;

		static InflaterHuffmanTree()
		{
			MAX_BITLEN = 15;
			try
			{
				byte[] array = new byte[288];
				int num = 0;
				while (num < 144)
				{
					array[num++] = 8;
				}
				while (num < 256)
				{
					array[num++] = 9;
				}
				while (num < 280)
				{
					array[num++] = 7;
				}
				while (num < 288)
				{
					array[num++] = 8;
				}
				defLitLenTree = new InflaterHuffmanTree(array);
				array = new byte[32];
				num = 0;
				while (num < 32)
				{
					array[num++] = 5;
				}
				defDistTree = new InflaterHuffmanTree(array);
			}
			catch (Exception)
			{
				throw new ApplicationException("InflaterHuffmanTree: static tree length illegal");
			}
		}

		public InflaterHuffmanTree(byte[] codeLengths)
		{
			BuildTree(codeLengths);
		}

		private void BuildTree(byte[] codeLengths)
		{
			int[] array = new int[MAX_BITLEN + 1];
			int[] array2 = new int[MAX_BITLEN + 1];
			foreach (int num in codeLengths)
			{
				if (num > 0)
				{
					int[] array3;
					int[] array4 = (array3 = array);
					nint num2 = num;
					array4[num] = array3[num2] + 1;
				}
			}
			int num3 = 0;
			int num4 = 512;
			for (int j = 1; j <= MAX_BITLEN; j++)
			{
				array2[j] = num3;
				num3 += array[j] << 16 - j;
				if (j >= 10)
				{
					int num5 = array2[j] & 0x1FF80;
					int num6 = num3 & 0x1FF80;
					num4 += num6 - num5 >> 16 - j;
				}
			}
			tree = new short[num4];
			int num7 = 512;
			for (int num8 = MAX_BITLEN; num8 >= 10; num8--)
			{
				int num9 = num3 & 0x1FF80;
				num3 -= array[num8] << 16 - num8;
				int num10 = num3 & 0x1FF80;
				for (int k = num10; k < num9; k += 128)
				{
					tree[DeflaterHuffman.BitReverse(k)] = (short)((-num7 << 4) | num8);
					num7 += 1 << num8 - 9;
				}
			}
			for (int l = 0; l < codeLengths.Length; l++)
			{
				int num11 = codeLengths[l];
				if (num11 == 0)
				{
					continue;
				}
				num3 = array2[num11];
				int num12 = DeflaterHuffman.BitReverse(num3);
				if (num11 <= 9)
				{
					do
					{
						tree[num12] = (short)((l << 4) | num11);
						num12 += 1 << num11;
					}
					while (num12 < 512);
				}
				else
				{
					int num13 = tree[num12 & 0x1FF];
					int num14 = 1 << (num13 & 0xF);
					num13 = -(num13 >> 4);
					do
					{
						tree[num13 | (num12 >> 9)] = (short)((l << 4) | num11);
						num12 += 1 << num11;
					}
					while (num12 < num14);
				}
				array2[num11] = num3 + (1 << 16 - num11);
			}
		}

		public int GetSymbol(StreamManipulator input)
		{
			int num;
			int num2;
			if ((num = input.PeekBits(9)) >= 0)
			{
				if ((num2 = tree[num]) >= 0)
				{
					input.DropBits(num2 & 0xF);
					return num2 >> 4;
				}
				int num3 = -(num2 >> 4);
				int n = num2 & 0xF;
				if ((num = input.PeekBits(n)) >= 0)
				{
					num2 = tree[num3 | (num >> 9)];
					input.DropBits(num2 & 0xF);
					return num2 >> 4;
				}
				int availableBits = input.AvailableBits;
				num = input.PeekBits(availableBits);
				num2 = tree[num3 | (num >> 9)];
				if ((num2 & 0xF) <= availableBits)
				{
					input.DropBits(num2 & 0xF);
					return num2 >> 4;
				}
				return -1;
			}
			int availableBits2 = input.AvailableBits;
			num = input.PeekBits(availableBits2);
			num2 = tree[num];
			if (num2 >= 0 && (num2 & 0xF) <= availableBits2)
			{
				input.DropBits(num2 & 0xF);
				return num2 >> 4;
			}
			return -1;
		}
	}
}
