using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEditor;

public class MapWindow : EditorWindow
{
#region Constants
#endregion

	private int m_Width = 2;
	private int m_Height = 4;
	
	[MenuItem("Map/Create")]
	static void ShowWindow()
	{
		// Show only if map doesn't exist
		EditorWindow.GetWindow(typeof(MapWindow));
	}

	[MenuItem("Map/Load from xml")]
	static void LoadFromXml()
	{
		var level_manager = GameObject.Find("LevelManager").GetComponent<ja2.script.LevelManager>();
		ClearTerrain(level_manager.terrainManager);

		// Generate xml scene name
		string current_scene_xml = EditorApplication.currentScene.Substring(0, EditorApplication.currentScene.LastIndexOf(".")) + ".xml";
		var xml_reader = XmlReader.Create(current_scene_xml);
		xml_reader.Read();

		level_manager.LoadXml(xml_reader, new AssetDatabaseCustom());
		// Create all assets
		level_manager.CreateAssets(GetCurrentScenePath() + "/", new AssetDatabaseCustom());
	}

#region Operations
	//! Delete the active terrain manager stuff.
	private static void ClearTerrain(ja2.script.TerrainManager Terrain)
	{
		// Get ALL terrain partitions, not only referenced (there could
		// be the one left when some error occurred during destroying it,
		// and aren't referenced inside terrain manager anywhere).
		var terrain_partitions_all = Terrain.GetComponentsInChildren<ja2.script.TerrainPartition>();
		foreach (var terrain_partition in terrain_partitions_all)
		{
			// Destroy mesh as first
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(terrain_partition.GetComponent<MeshFilter>().sharedMesh));
			// Destroy GO
			GameObject.DestroyImmediate(terrain_partition.gameObject);
		}
	}

	//! Get current scene path.
	private static string GetCurrentScenePath()
	{
		string current_scene = EditorApplication.currentScene;
		string current_scene_path = current_scene.Substring(0, current_scene.LastIndexOf('/'));
		current_scene_path = current_scene_path.Substring(current_scene_path.LastIndexOf('/') + 1);

		string asset_parent_path = "Assets/Resources/Scenes";
		string asset_path = asset_parent_path + "/" + current_scene_path;

		return asset_path;
	}
#endregion
#region Messages
	// Use this for initialization
	void OnGUI ()
	{
		EditorGUILayout.PrefixLabel("Map Settings", EditorStyles.boldLabel);
		// Add Width, height controls
		m_Width = (int)EditorGUILayout.IntField("Width", m_Width);
		m_Height = (int)EditorGUILayout.IntField("Height", m_Height);
		
		if(GUILayout.Button("Create"))
		{
			var level_manager = GameObject.Find("LevelManager").GetComponent<ja2.script.LevelManager>();
			// If we got some terrain already created, destroy it
			if (level_manager.terrainManager != null)
				ClearTerrain(level_manager.terrainManager);

			// Get current scene path
			string current_scene = EditorApplication.currentScene;
			string current_scene_path = current_scene.Substring(0, current_scene.LastIndexOf('/'));
			current_scene_path = current_scene_path.Substring(current_scene_path.LastIndexOf('/') + 1);
			// Material manager
			var material_manager = new ja2.TerrainMaterialManager(Application.dataPath);
			
			string asset_parent_path = "Assets/Resources/Scenes";
			string asset_path = GetCurrentScenePath();
			// Be sure asset path exist
			string asset_path_guid = AssetDatabase.AssetPathToGUID(asset_path);
			if(asset_path_guid.Length == 0)
				AssetDatabase.CreateFolder(asset_parent_path, current_scene_path);
			// Create terrain manager and all partitions
			var tile_set = material_manager.GetTerrainSet("summer");
			level_manager.terrainManager.This((ushort)m_Width, (ushort)m_Height, tile_set);
			// Create all assets
			level_manager.CreateAssets(asset_path + "/", new AssetDatabaseCustom());

			// Terrain manager has changed
			EditorUtility.SetDirty(level_manager.terrainManager);
			// Refresh asset database because of terrain were added there
			AssetDatabase.Refresh();
		}
	}
#endregion
}
