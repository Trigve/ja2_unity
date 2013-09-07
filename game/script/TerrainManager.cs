using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using System;
using ja2;

//! Terrain component for terrain GO.
[Serializable]
public class TerrainManager : MonoBehaviourEx, ja2.ITerrainManager
{
#region Constants
	//! Partition width preferred.
	public const byte PARTITION_WIDTH = 10;
	//! Partition height preferred.
	public const byte PARTITION_HEIGHT = 20;
	//! Tile width.
	public const float TILE_WIDTH = 0.7071067F;
	//! Tile width.
	public const float TILE_HEIGHT = TILE_WIDTH;
#endregion

#region Attributes
	//! Map.
	private ja2.Map map_;
	//! Terrain partitions;
	[SerializeField]
	private GameObject[] m_Partitions;
#endregion

#region Properties
	public ja2.Map map
	{
		get
		{
			return map_;
		}
		set
		{
			map_ = value;
		}
	}
	
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
	public Vector3 GetPosition(ja2.TerrainTile Tile, short Vertex)
	{
		// Get the partition used
		int partition_x = Tile.x / TerrainManager.PARTITION_WIDTH;
		int partition_y = Tile.y / TerrainManager.PARTITION_HEIGHT;
		// Compute normalized terrain tile position
		int normalized_x = Tile.x - TerrainManager.PARTITION_WIDTH * partition_x;
		int normalized_y = Tile.y - TerrainManager.PARTITION_HEIGHT * partition_y;

		GameObject terrain_go = m_Partitions[partition_x + partition_y * (map_.width / PARTITION_WIDTH)];

		return terrain_go.transform.TransformPoint(Terrain.TileVertex(normalized_x, normalized_y, Vertex));
	}

	//! Get position of center of tile.
	public Vector3 GetPosition(ja2.TerrainTile Tile)
	{
		return new Vector3(GetPosition(Tile, 1).x, 0, GetPosition(Tile, 0).z);
	}
#endregion
}
