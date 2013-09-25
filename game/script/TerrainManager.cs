using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using ja2;
using UnityEngine;

namespace ja2.script
{
	//! Terrain component for terrain GO.
	[Serializable]
	public class TerrainManager : MonoBehaviourEx
	{
#region Attributes
		//! Map.
		//	private ja2.Map map_;
		//! Map width in partition count.
		private ushort m_Width;
		//! Map height in partition count.
		private ushort m_Height;
		//! Terrain partitions;
		[SerializeField]
		public GameObject[] m_Partitions;
#endregion

#region Properties
		public ushort width { get { return m_Width; } }
		public ushort height { get { return m_Height; } }
		public GameObject[] partitions
		{
			get
			{
				return m_Partitions;
			}
			set
			{
				m_Partitions = value;
			}

		}
#endregion

#region Interface
		//! Get tile position for given tile.
		public Vector3 GetPosition(ja2.TerrainTileHandle Tile, short Vertex)
		{
			return GetPartition((ushort)Tile.partitionX, (ushort)Tile.partitionY).transform.TransformPoint(TerrainPartition.TileVertex(Tile.x, Tile.y, Vertex));
		}

		//! Get position of center of tile.
		public Vector3 GetPosition(ja2.TerrainTileHandle Tile)
		{
			return new Vector3(GetPosition(Tile, 1).x, 0, GetPosition(Tile, 0).z);
		}

		//! Get the all neighbors.
		/*!
			The tiles are returned in this order: NORTH, NORTH-EAST, EAST, SOUTH-EAST, SOUTH, SOUTH-WEST, WEST, NORTH-WEST.
		*/
		public ja2.TerrainTileHandle[] GetAllNeighbors(ja2.TerrainTileHandle Tile)
		{
			var output = new ja2.TerrainTileHandle[8];
			// Traverse all neighbors tiles
			for (Direction direction = Direction.NORTH; direction <= Direction.NORTH_WEST; direction = (Direction)(direction + 1))
			{
				// Add it to container if tile exist
				output[(int)direction - 1] = GetTile(Tile, direction);
			}

			return output;
		}

		//! Get tile instance.
		public ja2.TerrainTile GetTile(ja2.TerrainTileHandle TileHandle)
		{
			return GetPartition((ushort)TileHandle.partitionX, (ushort)TileHandle.partitionY).GetTile(TileHandle.x, TileHandle.y);
		}

		//! Get neighbor tile.
		public ja2.TerrainTileHandle GetTile(ja2.TerrainTileHandle Tile, Direction Dir)
		{
			int partition_x = Tile.partitionX;
			int partition_y = Tile.partitionY;
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
			// If we're past partition
			if (x >= TerrainPartition.PARTITION_WIDTH)
			{
				++partition_x;
				x %= TerrainPartition.PARTITION_WIDTH;
			}
			else if (x < 0)
			{
				--partition_x;
				x = TerrainPartition.PARTITION_WIDTH + x;
			}

			if (y >= TerrainPartition.PARTITION_HEIGHT)
			{
				++partition_y;
				y %= TerrainPartition.PARTITION_HEIGHT;
			}
			else if (y < 0)
			{
				--partition_y;
				y = TerrainPartition.PARTITION_HEIGHT + y;
			}

			return GetTileChecked(partition_x, partition_y, x, y);
		}

		//! Get tile in given direction and distance.
		public TerrainTileHandle GetTile(TerrainTileHandle Tile, ja2.Direction Dir, ushort Step)
		{
			TerrainTileHandle out_tile = Tile;
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
		public void SetTileTerrainType(ja2.TerrainTileHandle TileHandle, byte TerrainType)
		{
			ja2.TerrainTile tile = GetTile(TileHandle);
			// Set terrain type for base tile
			tile.SetTerrainType(TerrainTile.Vertex.NORTH, TerrainType);
			tile.SetTerrainType(TerrainTile.Vertex.WEST, TerrainType);
			tile.SetTerrainType(TerrainTile.Vertex.SOUTH, TerrainType);
			tile.SetTerrainType(TerrainTile.Vertex.EAST, TerrainType);
			// Must set for neighbors
			ja2.TerrainTileHandle[] near_neighbors = GetAllNeighbors(TileHandle);
			if (near_neighbors[0] != null)
				GetTile(near_neighbors[0]).SetTerrainType(TerrainTile.Vertex.SOUTH, TerrainType);
			if (near_neighbors[1] != null)
			{
				GetTile(near_neighbors[1]).SetTerrainType(TerrainTile.Vertex.WEST, TerrainType);
				GetTile(near_neighbors[1]).SetTerrainType(TerrainTile.Vertex.SOUTH, TerrainType);
			}
			if (near_neighbors[2] != null)
				GetTile(near_neighbors[2]).SetTerrainType(TerrainTile.Vertex.WEST, TerrainType);
			if (near_neighbors[3] != null)
			{
				GetTile(near_neighbors[3]).SetTerrainType(TerrainTile.Vertex.NORTH, TerrainType);
				GetTile(near_neighbors[3]).SetTerrainType(TerrainTile.Vertex.WEST, TerrainType);
			}
			if (near_neighbors[4] != null)
				GetTile(near_neighbors[4]).SetTerrainType(TerrainTile.Vertex.NORTH, TerrainType);
			if (near_neighbors[5] != null)
			{
				GetTile(near_neighbors[5]).SetTerrainType(TerrainTile.Vertex.NORTH, TerrainType);
				GetTile(near_neighbors[5]).SetTerrainType(TerrainTile.Vertex.EAST, TerrainType);
			}
			if (near_neighbors[6] != null)
				GetTile(near_neighbors[6]).SetTerrainType(TerrainTile.Vertex.EAST, TerrainType);
			if (near_neighbors[7] != null)
			{
				GetTile(near_neighbors[7]).SetTerrainType(TerrainTile.Vertex.SOUTH, TerrainType);
				GetTile(near_neighbors[7]).SetTerrainType(TerrainTile.Vertex.EAST, TerrainType);
			}
		}

		//! Add non-moveable for tile.
		public void AddNonMoveable(TerrainTileHandle Tile, NonMoveableObjectComponent NonMoveable)
		{
			GetPartition((ushort)Tile.partitionX, (ushort)Tile.partitionY).AssociateNonMoveable(Tile.x, Tile.y, NonMoveable);
		}

		//! Is tile walkable.
		public bool IsTileWalkable(TerrainTileHandle Tile)
		{
			return GetPartition((ushort)Tile.partitionX, (ushort)Tile.partitionY).IsWalkable(Tile.x, Tile.y);
		}
#endregion

#region Save/Load
		//! Save the data.
		public void Save(IFormatter Formatter, Stream Stream_, IAssetDatabase AssetDatabase)
		{
			Formatter.Serialize(Stream_, m_Width);
			Formatter.Serialize(Stream_, m_Height);

			// Serialize partitions
			//		Formatter.Serialize(Stream_, partitions.Length);
			foreach (var partition in partitions)
				partition.GetComponent<TerrainPartitionEditor>().Save(Formatter, Stream_, AssetDatabase);
		}

		//! Load the data.
		public void Load(IFormatter Formatter, Stream Stream_)
		{
			m_Width = (ushort)Formatter.Deserialize(Stream_);
			m_Height = (ushort)Formatter.Deserialize(Stream_);

			m_Partitions = new GameObject[m_Width * m_Height];
			// Create partitions
			for (int i = 0; i < m_Height; ++i)
			{
				for (int j = 0; j < m_Width; ++j)
				{
					GameObject terrain_go = script.TerrainLoader.CreateTerrainPartition(j, i, this);
					// Add to list of partitions
					m_Partitions[j + i * m_Width] = terrain_go;
					// Set parent
					TerrainPartition terrain_comp = terrain_go.GetComponent<TerrainPartition>();
					// Load the terrain partition data
					terrain_comp.Load(Formatter, Stream_);
				}
			}
		}
#endregion

#region Interface Static
		public static Direction GetDirection(ja2.TerrainTileHandle From, ja2.TerrainTileHandle To)
		{
			Direction dir = Direction.NONE;
			// Same partition
			{
				// Same y
				if (From.y == To.y)
				{
					if ((From.partitionX == To.partitionX && From.x < To.x) || From.partitionX < To.partitionX)
						dir = Direction.EAST;
					else
						dir = Direction.WEST;
				}
				else
				{
					int y_diff = From.y - To.y;
					int x_diff = From.x - To.x;

					int y_partition_diff = From.partitionY - To.partitionY;
					int x_partition_diff = From.partitionX - To.partitionX;

					if (y_partition_diff != 0)
					{
						if (y_partition_diff < 0)
							y_diff = TerrainPartition.PARTITION_HEIGHT - y_diff;
						else
							y_diff = y_diff + TerrainPartition.PARTITION_HEIGHT;
						// Need to inverse it, because it is inter-partition
						if (From.y % 2 == 1)
							y_diff *= -1;
					}
					if (x_partition_diff != 0)
					{
						if (x_partition_diff < 0)
							x_diff = TerrainPartition.PARTITION_WIDTH - x_diff;
						else
							x_diff = TerrainPartition.PARTITION_WIDTH + x_diff;
						
						if (From.y % 2 == 1)
							x_diff *= -1;
					}

					// Non-even y
					if (From.y % 2 == 1)
					{
						switch (y_diff)
						{
							// Row up
							case 1:
								if (x_diff == 0)
									dir = Direction.NORTH_WEST;
								else if (x_diff < 0)
									dir = Direction.NORTH_EAST;
								break;
							// Row down
							case -1:
								if (x_diff == 0)
									dir = Direction.SOUTH_WEST;
								else if (x_diff < 0)
									dir = Direction.SOUTH_EAST;
								break;
							// 2 Row up
							case 2:
								if (x_diff == 0)
									dir = Direction.NORTH;
								break;
							// 2 Row down
							case -2:
								if (x_diff == 0)
									dir = Direction.SOUTH;
								break;
						}
					}
					else
					{
						switch (y_diff)
						{
							// Row up
							case 1:
								if (x_diff == 0)
									dir = Direction.NORTH_EAST;
								else if (x_diff > 0)
									dir = Direction.NORTH_WEST;
								break;
							// Row down
							case -1:
								if (x_diff == 0)
									dir = Direction.SOUTH_EAST;
								else if (x_diff > 0)
									dir = Direction.SOUTH_WEST;
								break;
							// 2 Row up
							case 2:
								if (x_diff == 0)
									dir = Direction.NORTH;
								break;
							// 2 Row down
							case -2:
								if (x_diff == 0)
									dir = Direction.SOUTH;
								break;
						}
					}
				}
			}

			return dir;
		}
#endregion

#region Operations
		//! Get the partition.
		private TerrainPartition GetPartition(ushort X, ushort Y)
		{
			return m_Partitions[X + Y * m_Width].GetComponent <TerrainPartition>();
		}

		//! Get tile checked
		private ja2.TerrainTileHandle GetTileChecked(int PartitionX, int PartitionY, int X, int Y)
		{
			if (PartitionX < 0 || PartitionY < 0 || PartitionX >= m_Width || PartitionY >= m_Height || X < 0 || Y < 0 || X >= TerrainPartition.PARTITION_WIDTH || Y >= TerrainPartition.PARTITION_HEIGHT)
				return null;

			//		return GetTile(PartitionX, PartitionY, X, Y);
			return new ja2.TerrainTileHandle(X, Y, PartitionX, PartitionY);
		}
#endregion

#region Construction
		protected void This(ushort Width, ushort Height, ja2.TerrainTileSet TileSet)
		{
			m_Width = Width;
			m_Height = Height;
			m_Partitions = new GameObject[m_Width * m_Height];
		}
#endregion
	}
} /*ja2.script*/