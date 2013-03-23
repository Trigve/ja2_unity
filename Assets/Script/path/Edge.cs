using UnityEngine;
using System.Collections;
using System;

namespace path
{
	[Serializable]
	public class Edge
	{
#region Attributes
		//! From.
		public int from;
		//! To.
		public int to;
#endregion

#region Construction
		public Edge(int From, int To)
		{
			from = From;
			to = To;
		}
#endregion
	}
} /*path*/