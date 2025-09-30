using UnityEngine;
using System.Collections;
using System;

namespace UFrame.Collections
{
    public interface FastEnumerable<T>
    {
        void Enumerate(FastList<T> output);
    }

    public class FastArray
    {
        internal static int BinarySearch(int[] arr, int index)
        {
            return BinarySearch(arr, 0, arr.Length, index);
        }

        internal static int BinarySearch(int [] arr, int startIndex, int endIndex, int value)
        {
            int low = startIndex;
            int high = endIndex - 1;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                int midVal = arr[mid];

                if (midVal < value)
                    low = mid + 1;
                else if (midVal > value)
                    high = mid - 1;
                else
                    return mid; // key found
            }
            return -(low + 1);  // key not found.
        }
        internal static void Copy(Array source, int startId, Array target, int targetId, int count)
        {
            var targetLength = target.Length;
            for (int i = 0; i < count; i++)
            {
                var targetIndex = targetId + i;
                if (targetLength > targetIndex)
                {
                    target.SetValue(source.GetValue(i + startId), targetIndex);
                }
            }
        }

        internal static void Resize<T>(ref T[] innerArray, int capacity)
        {
            if (innerArray.Length < capacity)
            {
                var array = new T[capacity];
                for (int i = 0; i < innerArray.Length; i++)
                {
                    array[i] = innerArray[i];
                }
                innerArray = array;
            }
        }

        internal static int IndexOf<T>(T[] innerArray, T item)
        {
            for (int i = 0; i < innerArray.Length; i++)
            {
                if (innerArray[i].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        internal static void Clear<T>(T[] innerArray, int index, int count)
        {
            for (int i = index; i < index + count; i++)
            {
                if (innerArray.Length > i && i >= 0)
                {
                    innerArray[i] = default(T);
                }
            }
        }

        internal static void Copy<T>(T[] source, T[] target, int count)
        {
            count = Math.Min(count, Math.Min(source.Length, target.Length));
            for (int i = 0; i < count; i++)
            {
                target[i] = source[i];
            }
        }
    }
}