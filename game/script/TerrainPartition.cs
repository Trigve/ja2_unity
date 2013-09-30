using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Xml;
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
		//! Terrain tile properties.
		protected Dictionary<ushort, ja2.TerrainTileProperty> m_Properties;
		//! Map between non-moveable and tile to which it belongs.
		/*!
			It is only for caching.
		*/
		[NonSerialized]
		protected Dictionary<string, ushort> m_NonMoveableMap = new Dictionary<string, ushort>();
#endregion

#region Interface
		//! Get tile.
		public ja2.TerrainTile GetTile(int X, int Y)
		{
			return m_Tiles[GetTileIndex(X, Y)];
		}

		public ja2.TerrainTile GetTile(int Triangle)
		{
			return m_Tiles[Triangle / 2];
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

		//! Create mesh for partition.
		public Mesh CreateMesh(ja2.TerrainTileSet TileSet)
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
				}
			}

			Mesh mesh = new Mesh();
			mesh.vertices = array_vec;
			mesh.triangles = array_tri;
			mesh.uv = uv1;
			mesh.uv2 = uv2;
			mesh.tangents = uv3;
			mesh.RecalculateNormals();

			// Set mesh and material
			GetComponent<MeshFilter>().mesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;
			GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/" + TileSet.materialName, typeof(Material)) as Material;
			// Save the mesh
			return mesh;
		}
#endregion

#region Interface Editor
		//! Create all assets.
		public void CreateAssets(string Path, IEditor AssetDatabase)
		{
			AssetDatabase.CreateAsset(GetComponent<MeshCollider>().sharedMesh, Path + m_PositionX.ToString() + "_" + m_PositionY.ToString() + ".asset");
		}
#endregion

#region Save/Load
		//! Save xml.
		public void SaveXml(XmlWriter Writer, IEditor AssetDatabase)
		{
			Writer.WriteStartElement("partition");

			Writer.WriteAttributeString("position_x", m_PositionX.ToString());
			Writer.WriteAttributeString("position_y", m_PositionY.ToString());

			// Write tiles
			Writer.WriteStartElement("tile_list");
			Array.ForEach(m_Tiles, Tile => Tile.SaveXml(Writer));
			Writer.WriteEndElement();

			// Write properties
			Writer.WriteStartElement("property_list");
			foreach (var property in m_Properties)
			{
				Writer.WriteStartElement("item");
				Writer.WriteAttributeString("index", property.Key.ToString());
				property.Value.SaveXml(Writer);
				Writer.WriteEndElement();
			}
			Writer.WriteEndElement();

			// Write nonmoveables
			var non_moveables = GetComponentsInChildren<NonMoveableObjectComponent>();
			Writer.WriteStartElement("nonmoveable_list");
			foreach (var non_moveable in non_moveables)
			{
				Writer.WriteStartElement("item");
				Writer.WriteAttributeString("tile", m_NonMoveableMap[non_moveable.nonMoveable.id.ToString()].ToString());
				non_moveable.SaveXml(Writer, AssetDatabase);
				Writer.WriteEndElement();
			}
			Writer.WriteEndElement();

			Writer.WriteEndElement();
		}

		//! Load xml.
		public void LoadXml(XmlReader Reader, IEditor AssetDatabase)
		{
			m_PositionX = int.Parse(Reader.GetAttribute("position_x"));
			m_PositionY = int.Parse(Reader.GetAttribute("position_y"));

			m_Tiles = new TerrainTile[PARTITION_WIDTH * PARTITION_HEIGHT];
			int i = 0;
			Reader.ReadToDescendant("tile");
			do 
			{
				byte[] v_types = new byte[]
				{
					byte.Parse(Reader.GetAttribute("type_0")),
					byte.Parse(Reader.GetAttribute("type_1")),
					byte.Parse(Reader.GetAttribute("type_2")),
					byte.Parse(Reader.GetAttribute("type_3"))
				};
				var tile = new TerrainTile(int.Parse(Reader.GetAttribute("x")), int.Parse(Reader.GetAttribute("y")), (TerrainTile.Type)int.Parse(Reader.GetAttribute("type")));
				
				tile.SetTerrainType(TerrainTile.Vertex.NORTH, v_types[0]);
				tile.SetTerrainType(TerrainTile.Vertex.WEST, v_types[1]);
				tile.SetTerrainType(TerrainTile.Vertex.SOUTH, v_types[2]);
				tile.SetTerrainType(TerrainTile.Vertex.EAST, v_types[3]);
				m_Tiles[i++] = tile;

			} while (Reader.ReadToNextSibling("tile"));

			var mesh_renderer = GetComponent<MeshRenderer>();

			m_Properties = new Dictionary<ushort, ja2.TerrainTileProperty>();

			Reader.ReadToFollowing("property_list");
			if (Reader.ReadToDescendant("item"))
			{
				do
				{
					var tile_property = new TerrainTileProperty();
					tile_property.nonMoveable = new NonMoveableObjectHandle(Reader.GetAttribute("nonmoveable"));

					m_Properties[ushort.Parse(Reader.GetAttribute("index"))] = tile_property;
				} while (Reader.ReadToNextSibling("item"));
			}

			Reader.ReadToFollowing("nonmoveable_list");
			if (Reader.ReadToDescendant("item"))
			{
				do
				{
					ushort tile_index = ushort.Parse(Reader.GetAttribute("tile"));
					GameObject non_moveable_go = AssetDatabase.InstantiatePrefab(Resources.LoadAssetAtPath(Reader.GetAttribute("prefab"), typeof(GameObject))) as GameObject;
					var comp = non_moveable_go.GetComponent<NonMoveableObjectComponent>();
					comp.LoadXml(Reader, AssetDatabase);

					TerrainTile tile = m_Tiles[tile_index];
					AssociateNonMoveable(tile.x, tile.y, comp);
				} while (Reader.ReadToNextSibling("item"));
			}
		}

		//! Save.
		public void Save(IFormatter Formatter, Stream Stream_, IEditor AssetDatabase)
		{
			Formatter.Serialize(Stream_, m_PositionX);
			Formatter.Serialize(Stream_, m_PositionY);
			Formatter.Serialize(Stream_, m_Tiles);
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

		//! Load the data.
		public void Load(IFormatter Formatter, Stream Stream_)
		{
			m_PositionX = (int)Formatter.Deserialize(Stream_);
			m_PositionY = (int)Formatter.Deserialize(Stream_);
			m_Tiles = (ja2.TerrainTile[])Formatter.Deserialize(Stream_);
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
		private static int GetVertexIndex(int X, int Y)
		{
			return X * 4 + Y * PARTITION_WIDTH * 4;
		}

		private static int GetTriIndex(int X, int Y)
		{
			return X * 6 + Y * PARTITION_WIDTH * 6;
		}
#endregion

#region Construction
		public void This(int PartitionX, int PartitionY, ja2.TerrainTileSet TileSet)
		{
			m_PositionX = PartitionX;
			m_PositionY = PartitionY;
			// Create tiles
			m_Tiles = new ja2.TerrainTile[PARTITION_WIDTH * PARTITION_HEIGHT];

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