namespace UFrame.Collections
{
	/// <summary>
	/// Experimenting with dictionary. Not yet functional.
	/// </summary>
	public class FastDictionary<TKey, TValue>
	{
		static int curDepth;
		static int bigIndex, smallIndex, normHash, leIndex;
		const int CollisionResolver = 1;
		const int CollisionDepth = 5;
		const int BucketCount = 64;
		const int BucketSize = 64;
		const int MaxItems = BucketCount * BucketSize;
		ulong[] bucketAllocation = new ulong[BucketCount];
		TValue[] bucketValues = new TValue[BucketCount * BucketSize];
		int[] bucketHashes = new int[BucketCount * BucketSize];
		//TKey[] bucketKeys = new TKey[BucketCount * BucketSize];

		public bool Add(TKey key, TValue value)
		{
			Prime();
			return _Add(key.GetHashCode(), value);
		}

		private bool _Add(int hashCode, TValue item)
		{
			if (ForceStop)
				return false;
			GenerateIndexes(hashCode);
			if (GetBit(bucketAllocation[bigIndex], smallIndex))
			{
				if (bucketHashes[leIndex] == hashCode)
				{
					return false;
				}
				//Resolve collision
				return _Add(hashCode * CollisionResolver, item);
			}
			SetBitTrue(ref bucketAllocation[bigIndex], smallIndex);
			bucketValues[leIndex] = item;
			bucketHashes[leIndex] = hashCode;
			return true;
		}

        /// <summary>
        /// Get the value of the bit at bitIndex
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        /// <returns></returns>
        public static bool GetBit(ulong mask, int bitIndex)
        {
            return (mask & ((ulong)1 << bitIndex)) != 0;
        }


        //ulong mask
        /// <summary>
        /// Sets the value at bitIndex of a 64 bit mask to true
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        public static void SetBitTrue(ref ulong mask, int bitIndex)
        {
            mask |= (ulong)1 << bitIndex;
        }
        /// <summary>
        /// Sets the value at bitIndex of a 64 bit mask to false
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        public static void SetBitFalse(ref ulong mask, int bitIndex)
        {
            mask &= ~((ulong)1 << bitIndex);
        }

        public bool Remove(TKey key)
		{
			Prime();
			return _Remove(key.GetHashCode());
		}

		private bool _Remove(int hashCode)
		{
			if (ForceStop)
				return false;
			if (ConfirmSlot(hashCode))
			{
				SetBitFalse(ref bucketAllocation[bigIndex], smallIndex);
				return true;
			}
			return _Remove(hashCode * CollisionResolver);
		}

		public TValue this[TKey key]
		{
			get
			{
				Prime();
				return _GetValue(key.GetHashCode());
			}
		}

		private TValue _GetValue(int hashCode)
		{
			if (ForceStop)
				throw new System.IndexOutOfRangeException();
			if (ConfirmSlot(hashCode))
			{
				return bucketValues[leIndex];
			}
			return _GetValue(hashCode * CollisionResolver);
		}

		public bool TryGetValue(TKey key, out TValue output)
		{
			Prime();
			return _TryGetValue(key.GetHashCode(), out output);
		}

		private bool _TryGetValue(int hashCode, out TValue output)
		{
			if (ForceStop)
			{
				output = default(TValue);
				return false;
			}
			if (ConfirmSlot(hashCode))
			{
				output = bucketValues[leIndex];
				return true;
			}
			return _TryGetValue(hashCode * CollisionResolver, out output);
		}

		public bool ContainsKey(TKey key)
		{
			Prime();
			return _ContainsKey(key.GetHashCode());
		}

		private bool _ContainsKey(int hashCode)
		{
			if (ForceStop)
				return false;
			GenerateIndexes(hashCode);
			if (ConfirmSlot(hashCode))
				return true;
			return _ContainsKey(hashCode * CollisionResolver);
		}

		private static void Prime()
		{
			curDepth = 0;
		}

		private static bool ForceStop
		{
			get { return (curDepth++ >= CollisionDepth); }
		}

		private static void GenerateIndexes(int hashCode)
		{
			normHash = hashCode % MaxItems;
			bigIndex = normHash / BucketCount;
			smallIndex = normHash % BucketSize;
			leIndex = smallIndex * BucketCount + bigIndex;
		}

		private bool ConfirmSlot(int hashCode)
		{
			GenerateIndexes(hashCode);
			if (GetBit(bucketAllocation[bigIndex], smallIndex))
			{
				if (bucketHashes[leIndex] == hashCode)
				{
					return true;
				}
			}
			return false;
		}
	}
}
