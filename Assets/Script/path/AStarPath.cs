using UnityEngine;
using System.Collections.Generic;
using System;

namespace path
{
	public abstract class AStarPath<K> where K : IEquatable<K>
	{
#region Enums
		public enum State
		{
			WAIT,
			DONE,
			FAIL,
		}
#endregion
		//! Helper struct.
		protected struct NodeTag : IComparable<NodeTag>
		{
#region Constants
			//! Epsilon for comparison
			private const float EPSILON = 0.001f;
#endregion

#region Attributes
			//! f(x) value.
			public readonly float f_x;
			//! Tile used.
			public readonly K tile;
#endregion

#region Operations
			public int CompareTo(NodeTag Tag)
			{
				return f_x.CompareTo(Tag.f_x);
			}
#endregion

#region Construction
			public NodeTag(float F_x, K Tile)
			{
				f_x = F_x;
				tile = Tile;
			}
#endregion
		}

#region Attributes
		//! Start.
		protected readonly K start;
		//! End;
		protected readonly K end_;
		//! Initial estimate of path.
		protected float initialEstimate;
		//! Open set.
		protected System.Collections.Generic.HashSet<K> openSet = new System.Collections.Generic.HashSet<K>();
		//! Priority queue for open set.
		private utils.PriorityQueue<NodeTag> openSetPriority = new utils.PriorityQueue<NodeTag>();
		//! Closed set.
		protected System.Collections.Generic.HashSet<K> closedSet = new System.Collections.Generic.HashSet<K>();
		//! g(x) - Cost from starting point to x.
		private System.Collections.Generic.Dictionary<K, float> g_score = new System.Collections.Generic.Dictionary<K, float>();
		//! f(x) - Cost estimated from x to goal.
		protected System.Collections.Generic.Dictionary<K, float> f_score = new System.Collections.Generic.Dictionary<K, float>();
		//! Path store.
		private System.Collections.Generic.Dictionary<K, K> cameFrom = new System.Collections.Generic.Dictionary<K, K>();
		//! Result.
		protected K[] result { get; private set; }
#endregion

#region Properties
		//! Is path generation finished.
		public bool finished { get; private set; }
#endregion

#region Operations
		//! Start.
		protected void Start()
		{
			// Reset for start node
			g_score[start] = 0;
			// Recalculate estimate distance.
			f_score[start] = g_score[start] + Heurestic(start, end_);

			initialEstimate = f_score[start];

			// Push starting node
			openSet.Add(start);
			openSetPriority.Push(new NodeTag(f_score[start], start));
		}

		//! Calculate path.
		public State RunOnce()
		{
			// While we got something
			while (openSet.Count > 0)
			{
				K current = openSetPriority.Pop().tile;
				openSet.Remove(current);
				// Got the goal
				if (current.Equals(end_))
				{
					result = ReconstructPath(cameFrom, end_).ToArray();
					return State.DONE;
				}
				closedSet.Add(current);
				// Get tile from current
				foreach (K neigh in GetNeigbours(current))
				{
					float tentative_g = g_score[current] + Distance(current, neigh);
					// Got some tile in closed set
					if (closedSet.Contains(neigh))
					{
						// If actual g(x) is bigger, no need to examine
						if (tentative_g >= g_score[neigh])
							continue;
					}

					// New tile to examine or g(x) has got better
					if (!openSet.Contains(neigh) || tentative_g < g_score[neigh])
					{
						cameFrom[neigh] = current;
						g_score[neigh] = tentative_g;
						f_score[neigh] = tentative_g + Heurestic(neigh, end_);

						if (!openSet.Contains(neigh))
						{
							openSet.Add(neigh);
							openSetPriority.Push(new NodeTag(f_score[neigh], neigh));
						}
					}
				}

				return State.WAIT;
			}

			return State.FAIL;
		}

		//! Get estimated cost.
		protected abstract float Heurestic(K From, K To);

		//! Get real distance.
		protected abstract float Distance(K From, K To);

		//! Get all walkable neighbours.
		protected abstract K[] GetNeigbours(K Node);

		//! Generate actual path.
		private System.Collections.Generic.List<K> ReconstructPath(System.Collections.Generic.Dictionary<K, K> CameFrom, K Current)
		{
			if (CameFrom.ContainsKey(Current))
			{
				var path = ReconstructPath(CameFrom, CameFrom[Current]);
				path.Add(Current);
				return path;
			}
			else
				return new System.Collections.Generic.List<K>(new K[] { Current });
		}
#endregion

#region Construction
		protected AStarPath(K From, K To)
		{
			start = From;
			end_ = To;
		}
#endregion
	}
}