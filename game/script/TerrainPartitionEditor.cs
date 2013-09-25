using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using UnityEngine;

namespace ja2.script
{
	//! Terrain partition used with editor.
	[Serializable]
	public sealed class TerrainPartitionEditor : TerrainPartition
	{
#region Interface
		//! Refresh the mesh.
		public void Refresh(ja2.TerrainTileSet TileSet, IAssetDatabase AssetDatabase)
		{
			// Get actual mesh and associated path of asset
			Mesh old_mesh = GetComponent<MeshFilter>().sharedMesh;
			string asset_path = AssetDatabase.GetAssetPath(old_mesh);
			// Create new mesh
			Mesh mesh = CreateMesh(TileSet);
			// Update the mesh
			AssetDatabase.CreateAsset(mesh, asset_path);
			// Set mesh and material
			GetComponent<MeshFilter>().mesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;
		}

		//! Create mesh for partition.
		public void CreateMesh(ja2.TerrainTileSet TileSet, string AssetPath, IAssetDatabase AssetDatabase)
		{
			// Create mesh and save it
			Mesh mesh = CreateMesh(TileSet);
			AssetDatabase.CreateAsset(mesh, AssetPath + m_PositionX.ToString() + "_" + m_PositionY.ToString() + ".asset");
			// Set mesh and material
			GetComponent<MeshFilter>().mesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;
			GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/" + TileSet.materialName, typeof(Material)) as Material;
		}
#endregion

#region Operations
		private static int GetVertexIndex(int X, int Y)
		{
			return X * 4 + Y * PARTITION_WIDTH * 4;
		}

		private static int GetTriIndex(int X, int Y)
		{
			return X * 6 + Y * PARTITION_WIDTH * 6;
		}

		//! Create mesh for partition.
		private Mesh CreateMesh(ja2.TerrainTileSet TileSet)
		{
			// Create vertex array
			Vector3[] array_vec = new Vector3[PARTITION_WIDTH * PARTITION_HEIGHT * 4];
			// Create triangles array
			int[] array_tri = new int[PARTITION_WIDTH * PARTITION_HEIGHT * 6];
			// Create UV arrays
			Vector2[] uv1 = new Vector2[array_vec.Length];
			Vector2[] uv2 = new Vector2[array_vec.Length];
			Vector4[] uv3 = new Vector4[array_vec.Length];

			// Go through all tiles
			for (int i = 0, last_vertex = 0; i < PARTITION_HEIGHT; ++i)
			{
				for (int j = 0; j < PARTITION_WIDTH; ++j)
				{
					// Compute actual tile index for arry
					int tile_index = j + i * PARTITION_WIDTH;
					// Get Tile
					ja2.TerrainTile tile = m_Tiles[tile_index];
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
					ja2.TextureAtlasInfo primary_mat_info = TileSet.GetTileType(mat_1, tile.variant);
					// Get secondary tile type information
					ja2.TextureAtlasInfo secondary_mat_info = TileSet.GetTileType(mat_2, tile.variant);
					// Get the alpha splat info for tile
					ja2.TextureAtlasInfo alpha_splat_mat_info = TileSet.splatUsed.GetSplat(alpha_index);

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
					m_Mapping[triangle_index_raw] = (ushort)tile_index;
					m_Mapping[triangle_index_raw + 1] = (ushort)tile_index;
				}
			}

			Mesh mesh = new Mesh();
			mesh.vertices = array_vec;
			mesh.triangles = array_tri;
			mesh.uv = uv1;
			mesh.uv2 = uv2;
			mesh.tangents = uv3;
			mesh.RecalculateNormals();

			// Save the mesh
			return mesh;
		}
#endregion

#region Save/Load
		//! Save.
		public void Save(IFormatter Formatter, Stream Stream_, IAssetDatabase AssetDatabase)
		{
			Formatter.Serialize(Stream_, m_PositionX);
			Formatter.Serialize(Stream_, m_PositionY);
			Formatter.Serialize(Stream_, m_Tiles);
			Formatter.Serialize(Stream_, m_Mapping);
			Formatter.Serialize(Stream_, m_Properties);

			// Serialize mesh
			Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
			string asset_path = AssetDatabase.GetAssetPath(mesh);
			// Get only "Resources" relative path
			asset_path = asset_path.Substring(asset_path.IndexOf("Resources"));
			asset_path = asset_path.Substring(asset_path.IndexOf('/') + 1);
			asset_path = asset_path.Substring(0, asset_path.LastIndexOf('.'));

			Formatter.Serialize(Stream_, asset_path);

			// Serialize materials
			var materials = GetComponent<MeshRenderer>().sharedMaterials;
			Formatter.Serialize(Stream_, materials.Length);
			foreach (var mat in materials)
			{
				string material_path = AssetDatabase.GetAssetPath(mat);
				material_path = material_path.Substring(material_path.IndexOf("Resources"));
				material_path = material_path.Substring(material_path.IndexOf('/') + 1);
				material_path = material_path.Substring(0, material_path.LastIndexOf('.'));

				Formatter.Serialize(Stream_, material_path);
			}

			// Serialize all non-moveables
			var non_moveables = GetComponentsInChildren<NonMoveableObjectComponent>();
			foreach (var non_moveable in non_moveables)
				non_moveable.Save(Formatter, Stream_);
		}
#endregion
	}
} /*ja2.script*/
