using System.Collections;
using System;
using System.Collections.Generic;

namespace UFrame.HTN
{
	/// <summary>
	/// MTR is a metric of how costly a plan is. Nodes higher up in the hierarchy have a lower cost.
	/// </summary>
	[Serializable]
	public class MethodTraversalRecord : IComparable<MethodTraversalRecord>
	{
		/// <summary>
		/// List of traversal values.
		/// </summary>
		public List<int> list;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="values"></param>
		public MethodTraversalRecord()
		{
			list = new List<int>() { 0 };
		}

		public void Clear()
		{
			list.Clear();
		}

		/// <summary>
		/// Add cost.
		/// </summary>
		public void AddRange(List<int> values)
		{
			list.AddRange(values);
		}

		/// <summary>
		/// Increase cost for last value.
		/// </summary>
		public void Increment()
		{
			list[list.Count - 1]++;
		}

		public void CopyTo(MethodTraversalRecord other)
		{
			other.list.Clear();
			other.list.AddRange(list);
		}

		/// <summary>
		/// Compares to another MTR.
		/// </summary>
		/// <returns>Returns +1 if other is higher and -1 if other is lower.</returns>
		public int CompareTo(MethodTraversalRecord other)
		{
			if (other?.list == null)
			{
				return 0;
			}
			int num = Math.Min(list.Count, other.list.Count);
			for (int i = 0; i < num; i++)
			{
				int num2 = list[i];
				int num3 = other.list[i];
				if (num2 < num3)
				{
					return -1;
				}
				if (num2 > num3)
				{
					return 1;
				}
			}
			return list.Count.CompareTo(other.list.Count);
		}

	}
}
