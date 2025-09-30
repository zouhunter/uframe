using System;
using System.Collections;

namespace UFrame.Collections
{
	public class FastList<T> : FastEnumerable<T>, IEnumerable
	{
		private const int DefaultCapacity = 8;
		public T[] innerArray;
		public int Count { get; private set; } //Also the index of the next element to be added
		public int Capacity = DefaultCapacity;
		public bool IsValueType { get; private set; }

		public FastList(FastList<T> CopyList)
		{
			innerArray = (T[])CopyList.innerArray.Clone();
			Count = innerArray.Length;
			Capacity = innerArray.Length;
		}

		public FastList(T[] StartArray)
		{
			innerArray = StartArray;
			Count = innerArray.Length;
			Capacity = innerArray.Length;
		}

		public FastList(int StartCapacity)
		{
			Capacity = StartCapacity;
			innerArray = new T[Capacity];

			Initialize();
		}
		public FastList()
		{
			innerArray = new T[Capacity];
			Initialize();
		}

		private void Initialize()
		{

			Count = 0;
			this.IsValueType = typeof(T).IsValueType;
		}

		public void Add(T item)
		{
			EnsureCapacity(Count + 1);
			innerArray[Count++] = item;
		}

		public void AddRange(FastList<T> items)
		{
			int arrayLength = items.Count;
			EnsureCapacity(Count + arrayLength + 1);
			for (int i = 0; i < arrayLength; i++)
			{
				innerArray[Count++] = items[i];
			}
		}

		public void AddRange(T[] items)
		{
			int arrayLength = items.Length;
			EnsureCapacity(Count + arrayLength + 1);
			for (int i = 0; i < arrayLength; i++)
			{
				innerArray[Count++] = items[i];
			}
		}
		public void AddRange(T[] items, int startIndex, int count)
		{
			EnsureCapacity(Count + count + 1);
			for (int i = 0; i < count; i++)
			{
				innerArray[Count++] = items[i + startIndex];
			}
		}

		public bool Remove(T item)
		{
			var index = -1;
            for (int i = 0; i < innerArray.Length; i++)
            {
                if (item.Equals(innerArray[i]))
                {
					index = i;
					break;
				}
            }
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			Count--;
			var length = Count - 1;
			for (int i = index; i < length; i++)
            {
                innerArray[index] = innerArray[index + 1];
            }
		}

		public T[] ToArray()
		{
			T[] retArray = new T[Count];
            for (int i = 0; i < retArray.Length; i++)
            {
				retArray[i] = innerArray[i];
			}
			return retArray;
		}

		public bool Contains(T item)
		{
            for (int i = 0; i < innerArray.Length; i++)
            {
                if (item.Equals(innerArray[i]))
                {
					return true;
                }
            }
            return false;
		}

		public void Reverse()
		{
			int highCount = Count / 2;
			int reverseCount = Count - 1;
			for (int i = 0; i < highCount; i++)
			{
				T swapItem = innerArray[i];
				innerArray[i] = innerArray[reverseCount];
				innerArray[reverseCount] = swapItem;

				reverseCount--;
			}
		}

		public void EnsureCapacity(int min)
		{
			if (Capacity < min)
			{
				Capacity *= 2;
				if (Capacity < min)
				{
					Capacity = min;
				}
				FastArray.Resize(ref innerArray, Capacity);
			}
		}

		public T this[int index]
		{
			get
			{
				return innerArray[index];
			}
			set
			{
				innerArray[index] = value;
			}
		}

		public void Clear()
		{
			if (this.IsValueType)
			{
				FastClear();
			}
			else
			{
				for (int i = 0; i < Capacity; i++)
				{
					innerArray[i] = default(T);
				}
			}
			Count = 0;
		}

		/// <summary>
		/// Marks elements for overwriting. Note: this list will still keep references to objects.
		/// </summary>
		public void FastClear()
		{
			Count = 0;
		}

		public void CopyTo(FastList<T> target)
		{
            target.EnsureCapacity(Count);
            for (int i = 0; i < Count; i++)
            {
				target[i] = this[i];
			}
			target.Count = Count;
			target.Capacity = Capacity;
		}

		public T[] TrimmedArray
		{
			get
			{
				T[] ret = new T[Count];
				FastArray.Copy(innerArray, ret, Count);
				return ret;
			}
		}

		public override string ToString()
		{
			if (Count <= 0)
				return base.ToString();
			string output = string.Empty;
			for (int i = 0; i < Count - 1; i++)
				output += innerArray[i] + ", ";

			return base.ToString() + ": " + output + innerArray[Count - 1];
		}

		public void Enumerate(FastList<T> output)
		{
			output.FastClear();
			output.AddRange(this);
		}

        IEnumerator IEnumerable.GetEnumerator()
        {
			for (int i = 0; i < this.Count; i++)
            {
                yield return this.innerArray[i];
            }
        }
    }

}
