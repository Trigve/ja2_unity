using UnityEngine;
using System.Collections;
using System;

namespace path
{
	[Serializable]
	public sealed class Graph
	{
#region Attributes
		//! Array of nodes.
		public int[] nodes;
		//! Array of links.
		[SerializeField]
		public Edge[] edges;
		//! Array of weights.
		public int[] weights;
#endregion

#region Construction
		public Graph(int[] Nodes, Edge[] Edges, int[] Weights)
		{
			nodes = Nodes;
			edges = Edges;
			weights = Weights;
		}
#endregion
	}
} /*path*/
