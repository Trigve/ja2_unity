using System;
using System.Collections.Generic;

namespace utils
{
	public class PriorityQueue<T> where T : IComparable<T>
	{
#region Attributes
		private List<T> data = new List<T>();
#endregion
		
#region Properties
		public int size { get { return data.Count; } }
#endregion
		public void Push(T item)
		{
			data.Add(item);
			// child index; start at end
			int ci = data.Count - 1;
			while (ci > 0)
			{
				// parent index
				int pi = (ci - 1) / 2;
				// child item is larger than (or equal) parent so we're done
				if (data[ci].CompareTo(data[pi]) >= 0)
					break;
				T tmp = data[ci];
				data[ci] = data[pi];
				data[pi] = tmp;
				
				ci = pi;
			}
		}

		public T Pop()
		{
			// assumes pq is not empty; up to calling code
			// last index (before removal)
			int li = data.Count - 1;
			// fetch the front
			T frontItem = data[0];
			data[0] = data[li];
			data.RemoveAt(li);
			// last index (after removal)
			--li;
			// parent index. start at front of pq
			int pi = 0;
			while (true)
			{
				// left child index of parent
				int ci = pi * 2 + 1;
				// no children so done
				if (ci > li)
					break;
				// right child
				int rc = ci + 1;
				// if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
				if (rc <= li && data[rc].CompareTo(data[ci]) < 0)
					ci = rc;
				// parent is smaller than (or equal to) smallest child so done
				if (data[pi].CompareTo(data[ci]) <= 0)
					break;
				// swap parent and child
				T tmp = data[pi];
				data[pi] = data[ci];
				data[ci] = tmp;

				pi = ci;
			}
			return frontItem;
		}

		public T Peek()
		{
			T frontItem = data[0];
			return frontItem;
		}

		public override string ToString()
		{
			string s = "";
			for (int i = 0; i < data.Count; ++i)
				s += data[i].ToString() + " ";
			s += "count = " + data.Count;
			return s;
		}

		public bool IsConsistent()
		{
			// is the heap property true for all data?
			if (data.Count == 0)
				return true;
			// last index
			int li = data.Count - 1;
			// each parent index
			for (int pi = 0; pi < data.Count; ++pi)
			{
				// left child index
				int lci = 2 * pi + 1;
				// right child index
				int rci = 2 * pi + 2;
				// if lc exists and it's greater than parent then bad.
				if (lci <= li && data[pi].CompareTo(data[lci]) > 0)
					return false;
				// check the right child too.
				if (rci <= li && data[pi].CompareTo(data[rci]) > 0)
					return false;
			}
			return true; // passed all checks
		}
	}
}