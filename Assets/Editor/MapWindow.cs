using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class MapWindow : EditorWindow
{
#region Constants
	//! Partition name.
	private const string PARTITION_NAME = "Terrain_";
#endregion

	private int m_Width = 10;
	private int m_Height = 20;
	
	[MenuItem("Map/Create")]
	static void ShowWindow()
	{
		// Show only if map doesn't exist
		EditorWindow.GetWindow(typeof(MapWindow));
	}

#region Operations
	private void CreateTerrain(TerrainManager TerrainManager_, ja2.TerrainMaterialManager MatManager)
	{
		if (TerrainManager_.map.width % TerrainManager.PARTITION_WIDTH != 0 || TerrainManager_.map.width % TerrainManager.PARTITION_WIDTH != 0)
			throw new System.ArgumentException("Map width/height must be normalized to terrain partition width/height.");

		ja2.TerrainTileSet tile_set = MatManager.GetTerrainSet(TerrainManager_.map.terrainName);

		// All partitin GO
		var partitions = new List<GameObject>();
		// Need to create terrain partitions
		int partition_width = TerrainManager_.map.width / TerrainManager.PARTITION_WIDTH;
		int partition_height = TerrainManager_.map.height / TerrainManager.PARTITION_HEIGHT;
		for (int i = 0; i < partition_height; ++i)
		{
			for (int j = 0; j < partition_width; ++j)
			{
				string terrain_name = PARTITION_NAME + j + "_" + i;
				// Tile to vertex mapping
				var tile_map = new ja2.TerrainTileHandle[TerrainManager.PARTITION_WIDTH * TerrainManager.PARTITION_HEIGHT * 2];
				// Create terrain mesh
				Mesh mesh = CreatePartitionMesh(j, i, TerrainManager_.map, tile_set, tile_map);
				// Save the mesh as the asset
				AssetDatabase.CreateAsset(mesh, "Assets/terrain/" + terrain_name + ".asset");
				// Create terrain GO
				GameObject terrain_go = PrefabManagerEditor.Create("TerrainPartition");
				partitions.Add(terrain_go);
				terrain_go.name = terrain_name;
				// Set parent
				terrain_go.transform.parent = TerrainManager_.transform;
				// Update position
				Vector3 tile_vertex_0 = Terrain.TileVertex(j * TerrainManager.PARTITION_WIDTH, i * TerrainManager.PARTITION_HEIGHT, 0);
				Vector3 tile_vertex_1 = Terrain.TileVertex(j * TerrainManager.PARTITION_WIDTH, i * TerrainManager.PARTITION_HEIGHT, 1);
				terrain_go.transform.position = new Vector3(tile_vertex_0.x, 0, tile_vertex_1.z);
				// Set layer
				terrain_go.layer = Terrain.LAYER;
				// Set tile mapping
				terrain_go.GetComponent<Terrain>().mapping = tile_map;
				terrain_go.GetComponent<MeshFilter>().mesh = mesh;
				terrain_go.GetComponent<MeshCollider>().sharedMesh = mesh;
				var mesh_renderer = terrain_go.GetComponent<MeshRenderer>();
				// Set map material
				mesh_renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath("Assets/Materials/" + tile_set.materialName + ".mat", typeof(Material)) as Material;
			}
		}
		// Set all GOs
		TerrainManager_.partitions = partitions.ToArray();
	}

	public Mesh CreatePartitionMesh(int X, int Y, ja2.Map Map_, ja2.TerrainTileSet TileSet, ja2.TerrainTileHandle[] TileMap)
	{
		// Create vertex array
		Vector3[] array_vec = new Vector3[TerrainManager.PARTITION_WIDTH * TerrainManager.PARTITION_HEIGHT * 4];
		// Create triangles array
		int[] array_tri = new int[TerrainManager.PARTITION_WIDTH * TerrainManager.PARTITION_HEIGHT * 6];
		// Create UV arrays
		Vector2[] uv1 = new Vector2[array_vec.Length];
		Vector2[] uv2 = new Vector2[array_vec.Length];
		Vector4[] uv3 = new Vector4[array_vec.Length];
		
		// Go through all tiles
		for (int i = 0, last_vertex = 0; i < TerrainManager.PARTITION_HEIGHT; ++i)
		{
			for (int j = 0; j < TerrainManager.PARTITION_WIDTH; ++j)
			{
				// Get Tile
				ja2.TerrainTile tile = Map_.GetTile(j + X * TerrainManager.PARTITION_WIDTH, i + Y * TerrainManager.PARTITION_HEIGHT);
				// Create vertices
				array_vec[GetVertexIndex(j, i)] = Terrain.TileVertex(j, i, 0);
				array_vec[GetVertexIndex(j, i) + 1] = Terrain.TileVertex(j, i, 1);
				array_vec[GetVertexIndex(j, i) + 2] = Terrain.TileVertex(j, i, 2);
				array_vec[GetVertexIndex(j, i) + 3] = Terrain.TileVertex(j, i, 3);

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
				int triangle_index_raw = j * 2 + i * TerrainManager.PARTITION_WIDTH * 2;
				TileMap[triangle_index_raw] = new ja2.TerrainTileHandle(tile.x, tile.y);
				TileMap[triangle_index_raw + 1] = new ja2.TerrainTileHandle(tile.x, tile.y);
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

	private static int GetVertexIndex(int X, int Y)
	{
		return X * 4 + Y * TerrainManager.PARTITION_WIDTH * 4;
	}

	private static int GetTriIndex(int X, int Y)
	{
		return X * 6 + Y * TerrainManager.PARTITION_WIDTH * 6;
	}
#endregion

#region Messages
	// Use this for initialization
	void OnGUI ()
	{
		EditorGUILayout.PrefixLabel("Map Settings", EditorStyles.boldLabel);
		// Add Width, height controls
		m_Width = (int)EditorGUILayout.IntField("Width", m_Width);
		m_Height = (int)EditorGUILayout.IntField("Hieght", m_Height);
		
		if(GUILayout.Button("Create"))
		{
			var level_manager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
			// If we got some terrain already created, destroy it
			if (level_manager.terrainManager.map != null)
			{
				// Get ALL terrain partitions, not only referenced (there could
				// be the one left when some error occured during destroying it,
				// and aren't referenced inside terrain manager anywhere).
				var terrain_partitions_all = level_manager.terrainManager.GetComponentsInChildren<Terrain>();
				foreach (var terrain_partition in terrain_partitions_all)
				{
					// Destroy mesh as first
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(terrain_partition.GetComponent<MeshFilter>().sharedMesh));
					// Destroy GO
					GameObject.DestroyImmediate(terrain_partition.gameObject);
				}
			}
			// Create terrain
			level_manager.terrainManager.map = new ja2.Map(m_Width, m_Height, "summer");
			CreateTerrain(level_manager.terrainManager, new ja2.TerrainMaterialManager(Application.dataPath));
			// Terrain manager has changed
			EditorUtility.SetDirty(level_manager.terrainManager);
			// Refresh asset database because of terrain were added there
			AssetDatabase.Refresh();
		}
	}
#endregion
}
