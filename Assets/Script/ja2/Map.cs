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
			NONE = 0,
			NORTH = 1,
			NORTH_EAST = 2,
			EAST = 3,
			SOUTH_EAST = 4,
			SOUTH = 5,
			SOUTH_WEST = 6,
			WEST = 7,
			NORTH_WEST = 8,
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

		//! HACK!!!.
		public TerrainTile GetTile(int Index)
		{
			return m_Tiles[Index];
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
				output[(int)direction - 1] = GetTile(Tile, direction);
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

		public static Direction GetDirection(TerrainTile From, TerrainTile To)
		{
			Direction dir = Direction.NONE;
			// Same y
			if (From.y == To.y)
			{
				if (From.x < To.x)
					dir = Direction.EAST;
				else
					dir = Direction.WEST;
			}
			else
			{
				if (From.y % 2 == 1)
				{

					switch (From.y - To.y)
					{
						// Row up
						case 1:
							if (From.x == To.x)
								dir = Direction.NORTH_WEST;
							else if (From.x < To.x)
								dir = Direction.NORTH_EAST;
							break;
						// Row down
						case -1:
							if (From.x == To.x)
								dir = Direction.SOUTH_WEST;
							else if (From.x < To.x)
								dir = Direction.SOUTH_EAST;
							break;
						// 2 Row up
						case 2:
							if (From.x == To.x)
								dir = Direction.NORTH;
							break;
						// 2 Row down
						case -2:
							if (From.x == To.x)
								dir = Direction.SOUTH;
							break;
					}
				}
				else
				{
					switch (From.y - To.y)
					{
						// Row up
						case 1:
							if (From.x == To.x)
								dir = Direction.NORTH_EAST;
							else if (From.x > To.x)
								dir = Direction.NORTH_WEST;
							break;
						// Row down
						case -1:
							if (From.x == To.x)
								dir = Direction.SOUTH_EAST;
							else if (From.x > To.x)
								dir = Direction.SOUTH_WEST;
							break;
						// 2 Row up
						case 2:
							if (From.x == To.x)
								dir = Direction.NORTH;
							break;
						// 2 Row down
						case -2:
							if (From.x == To.x)
								dir = Direction.SOUTH;
							break;
					}
				}
			}

			return dir;
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
					// For first and last row and first and last collumn set
					// to non-walkable
					TerrainTile.Type tile_type = (i > 0 && j % 2 == 0) || (j % 2 == 1 && i < width - 1) ? TerrainTile.Type.REGULAR : TerrainTile.Type.NONE;
					TerrainTile tile = new TerrainTile(i, j, tile_type);
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
