using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using ja2;
using UnityEngine;
using UnityEditor;

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
			// As first get partition
			GameObject partition_go = GetPartition((ushort)TileHandle.partitionX, (ushort)TileHandle.partitionY);
			return partition_go.GetComponent<TerrainPartition>().GetTile(TileHandle.x, TileHandle.y);
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
				++partition_x;
			if (y >= TerrainPartition.PARTITION_HEIGHT)
				++partition_y;
			if (x < 0)
				--partition_x;
			if (y < 0)
				--partition_y;

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
#endregion

#region Save/Load
		//! Save the data.
		public void Save(IFormatter Formatter, Stream Stream_)
		{
			Formatter.Serialize(Stream_, m_Width);
			Formatter.Serialize(Stream_, m_Height);

			// Serialize partitions
			//		Formatter.Serialize(Stream_, partitions.Length);
			foreach (var partition in partitions)
				partition.GetComponent<TerrainPartition>().Save(Formatter, Stream_);
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
#endregion

#region Operations
		//! Get the partition.
		private GameObject GetPartition(ushort X, ushort Y)
		{
			return m_Partitions[X + Y * m_Width];
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
		public void This(ushort Width, ushort Height, ja2.TerrainTileSet TileSet, string SavePath)
		{
			m_Width = Width;
			m_Height = Height;
			m_Partitions = new GameObject[m_Width * m_Height];

			// Create partitions
			for (int i = 0; i < m_Height; ++i)
			{
				for (int j = 0; j < m_Width; ++j)
				{
					GameObject terrain_go = script.TerrainLoader.CreateTerrainPartition(j, i, this);
					// Add to list of partitions
					m_Partitions[j + i * m_Width] = terrain_go;
					// Call "constructor"				
					TerrainPartition terrain_comp = terrain_go.GetComponent<TerrainPartition>();
					terrain_comp.This(j, i, TileSet, SavePath);
				}
			}
		}
#endregion
	}
} /*ja2.script*/