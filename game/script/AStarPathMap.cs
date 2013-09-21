using UnityEngine;
using System.Collections.Generic;
using System;

namespace ja2.script
{
	//! Encapsulate terrain tile to be able used in hash container.
	public struct TileKey : IEquatable<TileKey>
	{
#region Fields
		//! Tile associated.
		public readonly ja2.TerrainTileHandle tile;
#endregion

		public bool Equals(TileKey Other)
		{
			return (tile.partitionX == Other.tile.partitionX && tile.partitionY == Other.tile.partitionY &&  tile.x == Other.tile.x && tile.y == Other.tile.y);
		}

		public override bool Equals(object obj)
		{
			return obj is TileKey ? Equals((TileKey)obj) : base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return (tile.partitionX << 24 | tile.partitionY << 16 | tile.x << 8 | tile.y);
		}

#region Construction
		public TileKey(ja2.TerrainTileHandle Tile)
		{
			tile = Tile;
		}
#endregion
	}

	public sealed class AStarPathMap : ja2.AStarPath<TileKey>
	{
#region Fields
		//! Terrain manager used.
		private TerrainManager terrainManager;
#endregion

#region Properties
		//! Get final tile.
		public ja2.TerrainTileHandle end { get { return base.end_.tile; } }
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
			ja2.TerrainTileHandle[] tiles = terrainManager.GetAllNeighbors(Key.tile);
			var tile_keys = new List<TileKey>();
			for (var i = 0; i < tiles.Length; ++i)
			{
				// Tile not walkable, dismiss
				if(tiles[i] != null && terrainManager.GetTile(tiles[i]).walkable())
					tile_keys.Add(new TileKey(tiles[i]));
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
					ret[i] = result[i].tile;
			}

			return ret;
		}

		//! Get closed nodes.
		public ja2.TerrainTileHandle[] ClosedSet()
		{
			var ret = new List<ja2.TerrainTileHandle>();

			foreach (TileKey item in closedSet)
				ret.Add(item.tile);

			return ret.ToArray();
		}

		//! Get open set.
		public utils.Tuple<float, ja2.TerrainTileHandle>[] OpenSet()
		{
			var ret = new List<utils.Tuple<float, ja2.TerrainTileHandle>>();

			foreach (var item in openSet)
				ret.Add(new utils.Tuple<float, ja2.TerrainTileHandle>(f_score[item], item.tile));

			return ret.ToArray();
		}
#endregion

#region Construction
		public AStarPathMap(TerrainManager TerrainManager_, ja2.TerrainTileHandle From, ja2.TerrainTileHandle To)
			: base(new TileKey(From), new TileKey(To))
		{
			terrainManager = TerrainManager_;
			// Must call start here because all is initialized now
			Start();
		}
#endregion
	}
} /*ja2.script*/
