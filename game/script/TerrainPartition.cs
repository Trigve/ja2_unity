using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine;

namespace ja2.script
{
	[Serializable]
	public class TerrainPartition : MonoBehaviour
	{
#region Constants
		//! Partition name.
		public const string PARTITION_NAME = "Terrain_";
		//! Partition width preferred.
		public const byte PARTITION_WIDTH = 10;
		//! Partition height preferred.
		public const byte PARTITION_HEIGHT = 20;
		//! Tile width.
		public const float TILE_WIDTH = 0.7071067F;
		//! Tile width.
		public const float TILE_HEIGHT = TILE_WIDTH;
		//! Layer index.
		public const int LAYER = 8;
		//! Layer mask
		public const int LAYER_MASK = 1 << 8;
#endregion

#region Fields
		//! Actual partition position x.
		protected int m_PositionX;
		//! Actual partition position y.
		protected int m_PositionY;
		//! All tiles.
		protected ja2.TerrainTile[] m_Tiles;
		//! Mapping between Tile and triangles.
		protected ushort[] m_Mapping;
		//! Terrain tile properties.
		protected Dictionary<ushort, ja2.TerrainTileProperty> m_Properties;
		//! Map between non-moveable and tile to which it belongs.
		/*!
			It is only for caching.
		*/
		[NonSerialized]
		protected Dictionary<string, ushort> m_NonMoveableMap = new Dictionary<string, ushort>();
#endregion

#region Properties
#endregion

#region Interface
		//! Get tile.
		public ja2.TerrainTile GetTile(int X, int Y)
		{
			return m_Tiles[GetTileIndex(X, Y)];
		}

		public ja2.TerrainTile GetTile(int Triangle)
		{
			return m_Tiles[m_Mapping[Triangle]];
		}

		//! Get tile for given triangle.
		public ja2.TerrainTileHandle GetTileHandle(int Triangle)
		{
			ja2.TerrainTile tile = GetTile(Triangle);

			return new ja2.TerrainTileHandle(tile.x, tile.y, m_PositionX, m_PositionY);
		}

		//! Get center of tile given by triangle.
		/*!
			\return global position.
		*/
		public Vector3 GetCenterOfTile(int Triangle)
		{
			return GetCenterOfTile(GetTile(Triangle));
		}

		//! Get center of tile given by triangle.
		/*!
			\return global position.
		*/
		public Vector3 GetCenterOfTile(int X, int Y)
		{
			return GetCenterOfTile(GetTile(X, Y));
		}

		//! Associate non-moveable to tile.
		public void AssociateNonMoveable(int X, int Y, NonMoveableObjectComponent NonMoveable)
		{
			// Set property
			GetCreateTileProperty(X, Y).nonMoveable = NonMoveable.nonMoveable.GetHandle();
			// Insert to map for better lookup
			m_NonMoveableMap[NonMoveable.nonMoveable.GetHandle().m_Id] = (ushort)GetTileIndex(X, Y);
			// Set parent
			NonMoveable.transform.parent = transform;
			// Set position
			NonMoveable.transform.position = GetCenterOfTile(X, Y);
			// Set parent
			NonMoveable.parent = this;
		}

		//! Check if tile is walkable
		public bool IsWalkable(int X, int Y)
		{
			bool walkable = true;

			// First try terrain tile
			TerrainTile tile = GetTile(X, Y);
			walkable = tile.walkable();
			// Check for nonmoveables
			if (walkable && PropertyExist(X, Y))
				walkable = (GetCreateTileProperty(X, Y).nonMoveable == null);

			return walkable;
		}

		//! Remove non-moveable.
		public void RemoveNonMoveable(NonMoveableObjectHandle NonMoveable)
		{
			ushort tile_index = m_NonMoveableMap[NonMoveable.m_Id];
			TerrainTileProperty terrain_prop = m_Properties[tile_index];
			// Must match
			if(terrain_prop.nonMoveable.m_Id != NonMoveable.m_Id)
				throw new ArgumentException("TerrainPartition: Nonmoveable object doesn't match when removing.");

			// Remove from property
			terrain_prop.nonMoveable = null;
		}
#endregion

#region Save/Load
		//! Load the data.
		public void Load(IFormatter Formatter, Stream Stream_)
		{
			m_PositionX = (int)Formatter.Deserialize(Stream_);
			m_PositionY = (int)Formatter.Deserialize(Stream_);
			m_Tiles = (ja2.TerrainTile[])Formatter.Deserialize(Stream_);
			// Load mappings
			m_Mapping = (ushort[])Formatter.Deserialize(Stream_);
			m_Properties = (Dictionary<ushort, ja2.TerrainTileProperty>)Formatter.Deserialize(Stream_);

			// Load mesh
			Mesh mesh = (Mesh)Resources.Load((string)Formatter.Deserialize(Stream_), typeof(Mesh));
			GetComponent<MeshFilter>().mesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;
			var mesh_renderer = GetComponent<MeshRenderer>();
			// Set map material
			int materials_length = (int)Formatter.Deserialize(Stream_);
			for (int i = 0; i < materials_length; ++i)
				mesh_renderer.sharedMaterial = Resources.Load((string)Formatter.Deserialize(Stream_), typeof(Material)) as Material;

			// Load all non-moveables
			foreach (var it in m_Properties)
			{
				if (it.Value.nonMoveable != null)
				{
					// Get tile
					var tile = m_Tiles[it.Key];
					GameObject non_moveable_go = utils.PrefabManager.Create("Items/Barrel");
					
					var comp = non_moveable_go.GetComponent<NonMoveableObjectComponent>();
					comp.Load(Formatter, Stream_);

					AssociateNonMoveable(tile.x, tile.y, comp);
				}
			}
		}
#endregion

#region Interface Static
		public static Vector3 TileVertex(int X, int Y, short Vertex)
		{
			Vector3 out_vec;

			float z_pos = ((Y % 2) * TILE_WIDTH) + TILE_WIDTH + X * TILE_WIDTH * 2;
			float x_pos = Y * TILE_WIDTH + TILE_WIDTH;

			switch (Vertex)
			{
				case 0:
					out_vec = new Vector3(x_pos - TILE_HEIGHT, 0, z_pos);
					break;
				case 1:
					out_vec = new Vector3(x_pos, 0, z_pos - TILE_WIDTH);
					break;
				case 2:
					out_vec = new Vector3(x_pos + TILE_HEIGHT, 0, z_pos);
					break;
				case 3:
					out_vec = new Vector3(x_pos, 0, z_pos + TILE_WIDTH);
					break;
				default:
					throw new ArgumentException("MapRenderer: Unknown tile vertex number.");
			}

			return out_vec;
		}
#endregion

#region Operations
		//! Get tile index.
		private int GetTileIndex(int X, int Y)
		{
			return (X + PARTITION_WIDTH * Y);
		}

		//! Property exist.
		private bool PropertyExist(int X, int Y)
		{
			return m_Properties.ContainsKey((ushort)GetTileIndex(X, Y));
		}

		//! Get or create tile property.
		private ja2.TerrainTileProperty GetCreateTileProperty(int X, int Y)
		{
			ushort tile_index = (ushort)GetTileIndex(X, Y);
			if (!m_Properties.ContainsKey(tile_index))
				m_Properties[tile_index] = new ja2.TerrainTileProperty();

			return m_Properties[tile_index];
		}

		//! Get center of tile given by triangle.
		/*!
			\return global position.
		*/
		private Vector3 GetCenterOfTile(TerrainTile Tile)
		{
			Vector3 v0 = TileVertex(Tile.x, Tile.y, 0);
			Vector3 v1 = TileVertex(Tile.x, Tile.y, 1);

			// Get center of tile
			return transform.TransformPoint(new Vector3(v1.x, 0, v0.z));

		}
#endregion

#region Construction
		public void This(int PartitionX, int PartitionY, ja2.TerrainTileSet TileSet, string SavePath)
		{
			m_PositionX = PartitionX;
			m_PositionY = PartitionY;
			// Create tiles
			m_Tiles = new ja2.TerrainTile[PARTITION_WIDTH * PARTITION_HEIGHT];
			// Create mappings; Size is double of tile size because for each tile
			// there are 2 triangles
			m_Mapping = new ushort[m_Tiles.Length * 2];

			System.Random rnd = new System.Random();

			for (int j = 0; j < PARTITION_HEIGHT; ++j)
			{
				for (int i = 0; i < PARTITION_WIDTH; ++i)
				{
					// Need to set all tiles as walkable, because we don't know
					// here which tiles are the ones at the border. It should
					// be set somewhere outside. \FIXME
					ja2.TerrainTile.Type tile_type = ja2.TerrainTile.Type.REGULAR;
					ja2.TerrainTile tile = new ja2.TerrainTile(i, j, tile_type);
					// Set random variant
					tile.variant = (byte)rnd.Next(0, 7);
					//				tile.variant = 0;
					m_Tiles[i + j * PARTITION_WIDTH] = tile;

				}
			}
			m_Properties = new Dictionary<ushort, ja2.TerrainTileProperty>();
			m_NonMoveableMap = new Dictionary<string, ushort>();
		}
#endregion
	}
} /*ja2.script*/