using UnityEngine;
using System.Collections.Generic;
using System;

namespace ja2.script
{
	public sealed class AStarPathMap : ja2.AStarPath<TerrainTileHandle>
	{
#region Fields
		//! Terrain manager used.
		private TerrainManager terrainManager;
#endregion

#region Properties
		//! Get final tile.
		public ja2.TerrainTileHandle end { get { return base.end_; } }
		//! Get final f(x).
		public float f_x_End { get { return initialEstimate; } }
#endregion

#region Overrides
		protected override float Heurestic(TerrainTileHandle From, TerrainTileHandle To)
		{
			Vector3 from_vec = terrainManager.GetPosition(From);
			Vector3 to_vec = terrainManager.GetPosition(To);

			return (to_vec - from_vec).magnitude;
		}

		protected override float Distance(TerrainTileHandle From, TerrainTileHandle To)
		{
			// Hack for now; 1 is constant distance cost .
			return 1;
		}

		protected override TerrainTileHandle[] GetNeigbours(TerrainTileHandle Key)
		{
			ja2.TerrainTileHandle[] tiles = terrainManager.GetAllNeighbors(Key);
			var tile_keys = new List<TerrainTileHandle>();
			for (var i = 0; i < tiles.Length; ++i)
			{
				// Tile not walkable, dismiss
				if (tiles[i] != null && terrainManager.IsTileWalkable(tiles[i]))
					tile_keys.Add(tiles[i]);
			}

			return tile_keys.ToArray();
		}
#endregion

#region Operations
		//! Get path.
		public ja2.TerrainTileHandle[] Path()
		{
			ja2.TerrainTileHandle[] ret = null;
			if (result != null)
			{
				ret = new ja2.TerrainTileHandle[result.Length];
				for (var i = 0; i < result.Length; ++i)
					ret[i] = result[i];
			}

			return ret;
		}

		//! Get closed nodes.
		public ja2.TerrainTileHandle[] ClosedSet()
		{
			var ret = new List<ja2.TerrainTileHandle>();

			foreach (TerrainTileHandle item in closedSet)
				ret.Add(item);

			return ret.ToArray();
		}

		//! Get open set.
		public utils.Tuple<float, ja2.TerrainTileHandle>[] OpenSet()
		{
			var ret = new List<utils.Tuple<float, ja2.TerrainTileHandle>>();

			foreach (var item in openSet)
				ret.Add(new utils.Tuple<float, ja2.TerrainTileHandle>(f_score[item], item));

			return ret.ToArray();
		}
#endregion

#region Construction
		public AStarPathMap(TerrainManager TerrainManager_, ja2.TerrainTileHandle From, ja2.TerrainTileHandle To)
			: base(From, To)
		{
			terrainManager = TerrainManager_;
			// Must call start here because all is initialized now
			Start();
		}
#endregion
	}
} /*ja2.script*/
