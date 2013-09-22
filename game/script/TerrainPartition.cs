using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace ja2.script
{
	[Serializable]
	public sealed class TerrainPartition : MonoBehaviour
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
		//! Layer index.
		public const int LAYER = 8;
		//! Layer mask
		public const int LAYER_MASK = 1 << 8;
#endregion

#region Fields
		//! Actual partition position x.
		private int m_PositionX;
		//! Actual partition position y.
		private int m_PositionY;
		//! All tiles.
		private ja2.TerrainTile[] m_Tiles;
		//! Mapping between Tile and triangles.
		private ushort[] m_Mapping;
		//! Terrain tile properties.
		private Dictionary<ushort, ja2.TerrainTileProperty> m_Properties;
		//! Non-moveable objects.
		private Dictionary<Guid, ja2.NonMoveableObject> m_NonMoveableObjects;
#endregion

#region Properties
		public ushort[] mapping
		{
			get
			{
				return m_Mapping;
			}
			set
			{
				m_Mapping = value;
			}
		}
#endregion

#region Interface
		//! Get tile.
		public ja2.TerrainTile GetTile(int X, int Y)
		{
			return m_Tiles[X + PARTITION_WIDTH * Y];
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
			ja2.TerrainTile tile = GetTile(Triangle);

			Vector3 v0 = TileVertex(tile.x, tile.y, 0);
			Vector3 v1 = TileVertex(tile.x, tile.y, 1);
			
			// Get center of tile
			return transform.TransformPoint(new Vector3(v1.x, 0, v0.z));
		}

		//! Refresh the mesh.
		public void Refresh(ja2.TerrainTileSet TileSet)
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
		public void CreateMesh(ja2.TerrainTileSet TileSet, string AssetPath)
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

#region Save/Load
		//! Save.
		public void Save(IFormatter Formatter, Stream Stream_)
		{
			Formatter.Serialize(Stream_, m_PositionX);
			Formatter.Serialize(Stream_, m_PositionY);
			Formatter.Serialize(Stream_, m_Tiles);
			Formatter.Serialize(Stream_, m_Mapping);

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
		}

		//! Load the data.
		public void Load(IFormatter Formatter, Stream Stream_)
		{
			m_PositionX = (int)Formatter.Deserialize(Stream_);
			m_PositionY = (int)Formatter.Deserialize(Stream_);
			m_Tiles = (ja2.TerrainTile[])Formatter.Deserialize(Stream_);
			// Load mappings
			m_Mapping = (ushort[])Formatter.Deserialize(Stream_);
			// Load mesh
			Mesh mesh = (Mesh)Resources.Load((string)Formatter.Deserialize(Stream_), typeof(Mesh));
			GetComponent<MeshFilter>().mesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;
			var mesh_renderer = GetComponent<MeshRenderer>();
			// Set map material
			int materials_length = (int)Formatter.Deserialize(Stream_);
			for (int i = 0; i < materials_length; ++i)
				mesh_renderer.sharedMaterial = Resources.Load((string)Formatter.Deserialize(Stream_), typeof(Material)) as Material;
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
			m_NonMoveableObjects = new Dictionary<Guid, ja2.NonMoveableObject>();
		}
#endregion
	}
} /*ja2.script*/