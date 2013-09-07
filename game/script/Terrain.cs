using UnityEngine;
using System.Collections;
using System;
using ja2;

[RequireComponent (typeof (MeshFilter), typeof (MeshRenderer), typeof (MeshCollider))]
[Serializable]
public sealed class Terrain : MonoBehaviour
{
#region Constants
	//! Tile width.
	public const float TILE_WIDTH = 0.7071067F;
	//! Tile width.
	public const float TILE_HEIGHT = TILE_WIDTH;
	//! Layer index.
	public const int LAYER = 8;
	//! Layer mask
	public const int LAYER_MASK = 1 << 8;
#endregion

#region Atrributes
	//! Mapping between Tile and triangles.
	private ja2.TerrainTileHandle[] m_Mapping;
#endregion

#region Properties
	public ja2.TerrainTileHandle[] mapping
	{
		set
		{
			m_Mapping = value;
		}
	}
#endregion

#region Operations
	//! Get tile for given triangle.
	public ja2.TerrainTileHandle GetTile(int Triangle)
	{
		return m_Mapping[Triangle];
	}

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
}
