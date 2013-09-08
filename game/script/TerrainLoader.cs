using System;
using System.Collections.Generic;
using UnityEngine;

namespace script
{
	//! Load the terrain.
	public sealed class TerrainLoader
	{
#region Constants
		//! Partition name.
		private const string PARTITION_NAME = "Terrain_";
#endregion
		
#region Delegates
		//! Mesh loader delegate.
		public delegate Mesh MeshLoader(int X, int Y, ja2.Map Map_, ja2.TerrainTileSet TileSet, out ja2.TerrainTileHandle[] TileMap);
#endregion

#region Interface
		//! Load the terrain to terrain.
		public static void CreateTerrain(TerrainManager TerrainManager_, ja2.TerrainTileSet TileSet, MeshLoader MeshLoaderFunc)
		{
			if (TerrainManager_.map.width % TerrainManager.PARTITION_WIDTH != 0 || TerrainManager_.map.width % TerrainManager.PARTITION_WIDTH != 0)
				throw new System.ArgumentException("Map width/height must be normalized to terrain partition width/height.");

			// All partition GO
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
					ja2.TerrainTileHandle[] tile_map;
					// Get the mesh
					Mesh mesh = MeshLoaderFunc(j, i, TerrainManager_.map, TileSet, out tile_map);
					// Create terrain GO
					GameObject terrain_go = utils.PrefabManager.Create("TerrainPartition");
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
					mesh_renderer.sharedMaterial = Resources.Load("Materials/" + TileSet.materialName, typeof(Material)) as Material;
				}
			}
			// Set all GOs
			TerrainManager_.partitions = partitions.ToArray();
		}
#endregion
	}
} /*script*/
