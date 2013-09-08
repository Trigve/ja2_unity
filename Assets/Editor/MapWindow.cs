using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class MapWindow : EditorWindow
{
#region Constants
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
	public Mesh CreatePartitionMesh(int X, int Y, ja2.Map Map_, ja2.TerrainTileSet TileSet, out ja2.TerrainTileHandle[] TileMap)
	{
		// Create tile map
		TileMap = new ja2.TerrainTileHandle[TerrainManager.PARTITION_WIDTH * TerrainManager.PARTITION_HEIGHT * 2];
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
			// Get current scene path
			string current_scene = EditorApplication.currentScene;
			string current_scene_path = current_scene.Substring(0, current_scene.LastIndexOf('/'));
			current_scene_path = current_scene_path.Substring(current_scene_path.LastIndexOf('/') + 1) + "/";
			// Material manager
			var material_manager = new ja2.TerrainMaterialManager(Application.dataPath);
			// Create terrain
			level_manager.terrainManager.map = new ja2.Map(m_Width, m_Height, "summer");
			script.TerrainLoader.CreateTerrain(level_manager.terrainManager, material_manager.GetTerrainSet(level_manager.terrainManager.map.terrainName), CreatePartitionMesh);
			// Terrain manager has changed
			EditorUtility.SetDirty(level_manager.terrainManager);
			// Refresh asset database because of terrain were added there
			AssetDatabase.Refresh();
		}
	}
#endregion
}
