using UnityEngine;
using System.Collections;
using System;

namespace ja2
{
	[Serializable]
	public sealed class TerrainPartition
	{
#region Constants
		//! Partition width preferred.
		public const byte PARTITION_WIDTH = 10;
		//! Partition height preferred.
		public const byte PARTITION_HEIGHT = 20;
#endregion
#region Inner classes
		[Serializable]
		//! Helper class.
		public class TriangleMap
		{
			public int x;
			public int y;

			public TriangleMap(int X, int Y)
			{
				this.x = X;
				this.y = Y;
			}
		}
#endregion

#region Attributes
		//! Tile width.
		public const float TILE_WIDTH = 0.7071067F;
		//! Tile width.
		public const float TILE_HEIGHT = TILE_WIDTH;
		//! Map for triangle -> Tile.
		[SerializeField]
		private TriangleMap[] m_TerrainMap;
#endregion


#region Operations
		public Mesh Create(int X, int Y, Map Map_, TerrainTileSet TileSet)
		{
			Map map = Map_;
			// Create vertex array
			Vector3[] array_vec = new Vector3[PARTITION_WIDTH * PARTITION_HEIGHT * 4];
			// Create triangles array
			int[] array_tri = new int[PARTITION_WIDTH * PARTITION_HEIGHT * 6];
			// Create UV arrays
			Vector2[] uv1 = new Vector2[array_vec.Length];
			Vector2[] uv2 = new Vector2[array_vec.Length];
			Vector4[] uv3 = new Vector4[array_vec.Length];
			// Triangle map
			m_TerrainMap = new TriangleMap[PARTITION_WIDTH * PARTITION_HEIGHT * 2];
			// Go through all tiles
			int partition_begin_x = 0;
			int partition_begin_y = 0;
			int partition_height = PARTITION_HEIGHT;
			int partition_width = PARTITION_WIDTH;
			for (int i = partition_begin_y, last_vertex = 0; i < partition_height; ++i)
			{
				for (int j = partition_begin_x; j < partition_width; ++j)
				{
					// Get Tile
					ja2.TerrainTile tile = map.GetTile(j + X * PARTITION_WIDTH, i + Y * PARTITION_HEIGHT);
					// Create vertices
					array_vec[GetVertexIndex(j, i)] = TileVertex(j, i, 0);
					array_vec[GetVertexIndex(j, i) + 1] = TileVertex(j, i, 1);
					array_vec[GetVertexIndex(j, i) + 2] = TileVertex(j, i, 2);
					array_vec[GetVertexIndex(j, i) + 3] = TileVertex(j, i, 3);

					last_vertex += 4;
					// Get the tile material types
					byte mat_v0 = tile.GetTerrainType(ja2.TerrainTile.Vertex.NORTH),
						mat_v1 = tile.GetTerrainType(ja2.TerrainTile.Vertex.WEST),
						mat_v2 = tile.GetTerrainType(ja2.TerrainTile.Vertex.SOUTH),
						mat_v3 = tile.GetTerrainType(ja2.TerrainTile.Vertex.EAST);
					// Get 1. and 2. material
					byte mat_1 = mat_v0;
					byte mat_2 = mat_1;
					if (mat_v0 != mat_v1)
						mat_2 = mat_v1;
					else if (mat_v0 != mat_v2)
						mat_2 = mat_v2;
					else if (mat_v0 != mat_v3)
						mat_2 = mat_v3;
					// Get alpha splat index
					byte alpha_index = 1;
					alpha_index |= (byte)((mat_v1 == mat_1) ? 2 : 0);
					alpha_index |= (byte)((mat_v2 == mat_1) ? 4 : 0);
					alpha_index |= (byte)((mat_v3 == mat_1) ? 8 : 0);
					// If materials need to be inverted
					if (alpha_index > 7)
					{
						byte mat_helper = mat_1;
						mat_1 = mat_2;
						mat_2 = mat_helper;
						alpha_index = (byte)(~alpha_index & 15);
					}
					// Get the primary tile type information
					TextureAtlasInfo primary_mat_info = TileSet.GetTileType(mat_1, tile.variant);
					// Get secondary tile type information
					TextureAtlasInfo secondary_mat_info = TileSet.GetTileType(mat_2, tile.variant);
					// Get the alpha splat info for tile
					TextureAtlasInfo alpha_splat_mat_info = TileSet.splatUsed.GetSplat(alpha_index);

					// Texture coordinates
					uv1[last_vertex - 4] = new Vector2(primary_mat_info.uvOffsetW + primary_mat_info.uvWidth / 2, primary_mat_info.uvOffsetH);
					uv1[last_vertex - 3] = new Vector2(primary_mat_info.uvOffsetW, primary_mat_info.uvOffsetH - primary_mat_info.uvHeight / 2);
					uv1[last_vertex - 2] = new Vector2(primary_mat_info.uvOffsetW + primary_mat_info.uvWidth / 2, primary_mat_info.uvOffsetH - primary_mat_info.uvHeight);
					uv1[last_vertex - 1] = new Vector2(primary_mat_info.uvOffsetW + primary_mat_info.uvWidth, primary_mat_info.uvOffsetH - primary_mat_info.uvHeight / 2);

					uv2[last_vertex - 4] = new Vector2(secondary_mat_info.uvOffsetW + secondary_mat_info.uvWidth / 2, secondary_mat_info.uvOffsetH);
					uv2[last_vertex - 3] = new Vector2(secondary_mat_info.uvOffsetW, secondary_mat_info.uvOffsetH - secondary_mat_info.uvHeight / 2);
					uv2[last_vertex - 2] = new Vector2(secondary_mat_info.uvOffsetW + secondary_mat_info.uvWidth / 2, secondary_mat_info.uvOffsetH - secondary_mat_info.uvHeight);
					uv2[last_vertex - 1] = new Vector2(secondary_mat_info.uvOffsetW + secondary_mat_info.uvWidth, secondary_mat_info.uvOffsetH - secondary_mat_info.uvHeight / 2);

					uv3[last_vertex - 4] = new Vector4(alpha_splat_mat_info.uvOffsetW + alpha_splat_mat_info.uvWidth / 2, alpha_splat_mat_info.uvOffsetH, 0);
					uv3[last_vertex - 3] = new Vector4(alpha_splat_mat_info.uvOffsetW, alpha_splat_mat_info.uvOffsetH - alpha_splat_mat_info.uvHeight / 2, 0, 0);
					uv3[last_vertex - 2] = new Vector4(alpha_splat_mat_info.uvOffsetW + alpha_splat_mat_info.uvWidth / 2, alpha_splat_mat_info.uvOffsetH - alpha_splat_mat_info.uvHeight, 0, 0);
					uv3[last_vertex - 1] = new Vector4(alpha_splat_mat_info.uvOffsetW + alpha_splat_mat_info.uvWidth, alpha_splat_mat_info.uvOffsetH - alpha_splat_mat_info.uvHeight / 2, 0, 0);
					// Create triangles
					int triangle_index = GetTriIndex(j, i);
					array_tri[triangle_index] = last_vertex - 4;
					array_tri[triangle_index + 1] = last_vertex - 1;
					array_tri[triangle_index + 2] = last_vertex - 3;

					array_tri[triangle_index + 3] = last_vertex - 3;
					array_tri[triangle_index + 4] = last_vertex - 1;
					array_tri[triangle_index + 5] = last_vertex - 2;
					// Save triangles
					int triangle_index_raw = j * 2 + i * PARTITION_WIDTH * 2;
					m_TerrainMap[triangle_index_raw] = new TriangleMap(tile.x, tile.y);
					m_TerrainMap[triangle_index_raw + 1] = new TriangleMap(tile.x, tile.y);
				}
			}

			Mesh mesh = new Mesh();
			mesh.vertices = array_vec;
			mesh.triangles = array_tri;
			mesh.uv = uv1;
			mesh.uv2 = uv2;
			mesh.tangents = uv3;
			mesh.RecalculateNormals();

			return mesh;
		}

		//! Find terrain tile.
		public TriangleMap GetTile(int TriangleIndex)
		{
			return m_TerrainMap[TriangleIndex];
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

		private int GetVertexIndex(int X, int Y)
		{
			return X * 4 + Y * PARTITION_WIDTH * 4;
		}

		private int GetTriIndex(int X, int Y)
		{
			return X * 6 + Y * PARTITION_WIDTH * 6;
		}
		#endregion
	}
}
