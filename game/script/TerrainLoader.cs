using System;
using System.Collections.Generic;
using UnityEngine;

namespace ja2.script
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
		public delegate Mesh MeshLoader(int X, int Y, TerrainManager Map_, string PathToSave, out ja2.TerrainTileHandle[] TileMap);
#endregion

#region Interface
		//! Create terrain.
		public static GameObject CreateTerrainPartition(int X, int Y, TerrainManager TerrainMan)
		{
			string terrain_name = PARTITION_NAME + X + "_" + Y;

			// Create terrain GO
			GameObject terrain_go = utils.PrefabManager.Create("TerrainPartition");
			terrain_go.name = terrain_name;
			// Set parent
			terrain_go.transform.parent = TerrainMan.transform;
			// Update position
			Vector3 tile_vertex_0 = TerrainPartition.TileVertex(X * TerrainPartition.PARTITION_WIDTH, Y * TerrainPartition.PARTITION_HEIGHT, 0);
			Vector3 tile_vertex_1 = TerrainPartition.TileVertex(X * TerrainPartition.PARTITION_WIDTH, Y * TerrainPartition.PARTITION_HEIGHT, 1);
			terrain_go.transform.position = new Vector3(tile_vertex_0.x, 0, tile_vertex_1.z);
			// Set layer
			terrain_go.layer = TerrainPartition.LAYER;

			return terrain_go;
		}
#endregion
	}
} /*ja2.script*/
