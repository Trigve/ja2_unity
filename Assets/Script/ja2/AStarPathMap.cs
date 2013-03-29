using UnityEngine;
using System.Collections.Generic;
using System;

namespace ja2
{
	//! Encapsulate terrain tile to be able used in hash container.
	public sealed class TileKey : IEquatable<TileKey>
	{
#region Attributes
		//! Tile associated.
		public readonly TerrainTile tile;
#endregion

		public bool Equals(TileKey Other)
		{
			return (tile.x == Other.tile.x && tile.y == Other.tile.y);
		}

		public override bool Equals(object obj)
		{
			return obj is TileKey ? Equals((TileKey)obj) : base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return (tile.x << 16 | tile.y);
		}

#region Construction
		public TileKey(ja2.TerrainTile Tile)
		{
			tile = Tile;
		}
#endregion
	}

	public sealed class AStarPathMap : path.AStarPath<TileKey>
	{
#region Attributes
		//! Terrain manager used.
		private TerrainManager terrainManager;
#endregion

#region Properties
		//! Get final tile.
		new public TerrainTile end { get { return base.end.tile; } }
		//! Get final f(x).
		public float f_x_End { get { return initialEstimate; } }
#endregion

#region Overrides
		protected override float Heurestic(TileKey From, TileKey To)
		{
			Vector3 from_vec = terrainManager.GetPosition(From.tile);
			Vector3 to_vec = terrainManager.GetPosition(To.tile);

			return (to_vec - from_vec).magnitude;
		}

		protected override float Distance(TileKey From, TileKey To)
		{
			// Hack for now; 1 is constant distance cost .
			return 1;
		}

		protected override TileKey[] GetNeigbours(TileKey Key)
		{
			ja2.TerrainTile[] tiles = terrainManager.map.GetAllNeighbors(Key.tile);
			var tile_keys = new List<TileKey>();
			for (var i = 0; i < tiles.Length; ++i)
			{
				// Tile not walkable, dismiss
				if(tiles[i] != null && tiles[i].walkable())
					tile_keys.Add(new TileKey(tiles[i]));
			}

			return tile_keys.ToArray();
		}
#endregion

#region Operations
		//! Get path.
		public ja2.TerrainTile[] Path()
		{
			ja2.TerrainTile[] ret = null;
			if (result != null)
			{
				ret = new ja2.TerrainTile[result.Length];
				for (var i = 0; i < result.Length; ++i)
					ret[i] = result[i].tile;
			}

			return ret;
		}

		//! Get closed nodes.
		public ja2.TerrainTile[] ClosedSet()
		{
			var ret = new List<ja2.TerrainTile>();

			foreach (TileKey item in closedSet)
				ret.Add(item.tile);

			return ret.ToArray();
		}

		//! Get open set.
		public Tuple<float, ja2.TerrainTile>[] OpenSet()
		{
			var ret = new List<Tuple<float, ja2.TerrainTile>>();

			foreach (var item in openSet)
				ret.Add(new Tuple<float, ja2.TerrainTile>(f_score[item], item.tile));

			return ret.ToArray();
		}
#endregion

#region Construction
		public AStarPathMap(TerrainManager TerrainManager_, TerrainTile From, TerrainTile To)
			: base(new TileKey(From), new TileKey(To))
		{
			terrainManager = TerrainManager_;
			// Must call start here because all is initialized now
			Start();
		}
#endregion
	}
} /*ja2*/
