using System;
using UnityEngine;

namespace ja2
{
	//! Map.
	[Serializable]
	public sealed class Map
	{
		#region Constants
		#endregion

		#region Enums
		//! Direction.
		public enum Direction
		{
			NORTH,
			NORTH_EAST,
			EAST,
			SOUTH_EAST,
			SOUTH,
			SOUTH_WEST,
			WEST,
			NORTH_WEST,
		};
		#endregion

#region Attributes
		//! Map width.
		public int width;
		//! Map height.
		public int height;
		//! Terrain name.
		public string terrainName;
		//! Terrain tiles.
		[SerializeField]
		private TerrainTile[] m_Tiles;
#endregion

#region Properties
		//! Number of tiles.
		public int size { get { return m_Tiles.Length; } }
#endregion

#region Operations
		//! Get tile.
		public TerrainTile GetTile(int X, int Y)
		{
			return m_Tiles[GetTileIndex(X, Y)];
		}
		//! Get tile checked
		private TerrainTile GetTileChecked(int X, int Y)
		{
			if (X < 0 || Y < 0 || GetTileIndex(X, Y) >= m_Tiles.Length)
				return null;

			return GetTile(X, Y);
		}

		//! Get tile index.
		public int GetTileIndex(int X, int Y)
		{
			return (X + Y * width);
		}

		//! Get the all neighbors.
		/*!
			The tiles are returned in this order: NORTH, NORTH-EAST, EAST, SOUTH-EAST, SOUTH, SOUTH-WEST, WEST, NORTH-WEST.
		*/
		public TerrainTile[] GetAllNeighbors(TerrainTile Tile)
		{
			TerrainTile[] output = new TerrainTile[8];
			// Traverse all neighbors tiles
			for (Direction direction = Direction.NORTH; direction <= Direction.NORTH_WEST; direction = (Direction)(direction + 1))
			{
				// Add it to container if tile exist
				output[(int)direction] = GetTile(Tile, direction);
			}

			return output;
		}
		//! Get neighbor tile.
		public TerrainTile GetTile(TerrainTile Tile, Direction Dir)
		{
			int x = Tile.x;
			int y = Tile.y;

			switch (Dir)
			{
				case Direction.NORTH:
					y -= 2;
					break;
				case Direction.NORTH_EAST:
					--y;
					x += (int)(Tile.y & 1);
					break;
				case Direction.EAST:
					++x;
					break;
				case Direction.SOUTH_EAST:
					++y;
					x += (int)(Tile.y & 1);
					break;
				case Direction.SOUTH:
					y += 2;
					break;
				case Direction.SOUTH_WEST:
					++y;
					x += (int)((Tile.y & 1) - 1);
					break;
				case Direction.WEST:
					--x;
					break;
				case Direction.NORTH_WEST:
					--y;
					x += (int)((Tile.y & 1) - 1);
					break;
			}

			return GetTileChecked(x, y);
		}

		//! Get tile in given direction and distance.
		public TerrainTile GetTile(TerrainTile Tile, Direction Dir, ushort Step)
		{
			TerrainTile out_tile = Tile;
			for (ushort i = 0; i < Step; ++i)
			{
				// Break if we are out
				if (out_tile == null)
					break;

				out_tile = GetTile(out_tile, Dir);
			}
			
			return out_tile;
		}
		//! Set tile terrain type.
		public void SetTileTerrainType(TerrainTile Tile, byte TerrainType)
		{
			// Set terrain type for base tile
			Tile.SetTerrainType(TerrainTile.Vertex.NORTH, TerrainType);
			Tile.SetTerrainType(TerrainTile.Vertex.WEST, TerrainType);
			Tile.SetTerrainType(TerrainTile.Vertex.SOUTH, TerrainType);
			Tile.SetTerrainType(TerrainTile.Vertex.EAST, TerrainType);
			// Must set for neighbors
			TerrainTile[] near_neighbors = GetAllNeighbors(Tile);
			if (near_neighbors[0] != null)
				near_neighbors[0].SetTerrainType(TerrainTile.Vertex.SOUTH, TerrainType);
			if (near_neighbors[1] != null)
			{
				near_neighbors[1].SetTerrainType(TerrainTile.Vertex.WEST, TerrainType);
				near_neighbors[1].SetTerrainType(TerrainTile.Vertex.SOUTH, TerrainType);
			}
			if (near_neighbors[2] != null)
				near_neighbors[2].SetTerrainType(TerrainTile.Vertex.WEST, TerrainType);
			if (near_neighbors[3] != null)
			{
				near_neighbors[3].SetTerrainType(TerrainTile.Vertex.NORTH, TerrainType);
				near_neighbors[3].SetTerrainType(TerrainTile.Vertex.WEST, TerrainType);
			}
			if (near_neighbors[4] != null)
				near_neighbors[4].SetTerrainType(TerrainTile.Vertex.NORTH, TerrainType);
			if (near_neighbors[5] != null)
			{
				near_neighbors[5].SetTerrainType(TerrainTile.Vertex.NORTH, TerrainType);
				near_neighbors[5].SetTerrainType(TerrainTile.Vertex.EAST, TerrainType);
			}
			if (near_neighbors[6] != null)
				near_neighbors[6].SetTerrainType(TerrainTile.Vertex.EAST, TerrainType);
			if (near_neighbors[7] != null)
			{
				near_neighbors[7].SetTerrainType(TerrainTile.Vertex.SOUTH, TerrainType);
				near_neighbors[7].SetTerrainType(TerrainTile.Vertex.EAST, TerrainType);
			}
		}
#endregion

		#region Constructor
		public Map(int Width, int Height, string TerrainName)
		{
			width = Width;
			height = Height;
			terrainName = TerrainName;

			System.Random rnd = new System.Random();
			// Create map
			m_Tiles = new TerrainTile[width * height];
			for (int j = 0; j < height; ++j)
			{
				for (int i = 0; i < width; ++i)
				{
					TerrainTile tile = new TerrainTile(i, j);
					// Set random variant
//					tile.variant = (byte)rnd.Next(0, 7);
					tile.variant = 0;

					m_Tiles[i + j * width] = tile;
				}
			}
			// Set some debug tile
			SetTileTerrainType(GetTile(2, 3), 1);
		}
		#endregion
	}
}
