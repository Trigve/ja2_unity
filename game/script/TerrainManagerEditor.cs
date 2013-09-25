using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ja2.script
{
	//! Terrain manager for use in editor.
	[Serializable]
	public sealed class TerrainManagerEditor : TerrainManager
	{
#region Operations
		private GameObject CreateTerrainPartition(int X, int Y)
		{
			string terrain_name = TerrainPartition.PARTITION_NAME + X + "_" + Y;

			// Create terrain GO
			GameObject terrain_go = utils.PrefabManager.Create("TerrainPartitionEditor");
			terrain_go.name = terrain_name;
			// Set parent
			terrain_go.transform.parent = transform;
			// Update position
			Vector3 tile_vertex_0 = TerrainPartition.TileVertex(X * TerrainPartition.PARTITION_WIDTH, Y * TerrainPartition.PARTITION_HEIGHT, 0);
			Vector3 tile_vertex_1 = TerrainPartition.TileVertex(X * TerrainPartition.PARTITION_WIDTH, Y * TerrainPartition.PARTITION_HEIGHT, 1);
			terrain_go.transform.position = new Vector3(tile_vertex_0.x, 0, tile_vertex_1.z);
			// Set layer
			terrain_go.layer = TerrainPartition.LAYER;

			return terrain_go;
		}
#endregion
#region Construction
		public void This(ushort Width, ushort Height, ja2.TerrainTileSet TileSet, string SavePath, IAssetDatabase AssetDatabase)
		{
			base.This(Width, Height, TileSet);

			// Create partitions
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					GameObject terrain_go = CreateTerrainPartition(j, i);
					// Add to list of partitions
					m_Partitions[j + i * width] = terrain_go;
					// Call "constructor"				
					var terrain_comp = terrain_go.GetComponent<TerrainPartitionEditor>();
					terrain_comp.This(j, i, TileSet, SavePath);
				}
			}
			System.Random rnd = new System.Random();
			// Set some random tile
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					var terrain_comp = m_Partitions[j + i * width].GetComponent<TerrainPartitionEditor>();

					for (int k = 0; k < 10; ++k)
					{
						SetTileTerrainType(
							new TerrainTileHandle(rnd.Next(0, TerrainPartition.PARTITION_WIDTH), rnd.Next(0, TerrainPartition.PARTITION_HEIGHT), j, i),
							(byte)rnd.Next(0, 2)
						);
					}
					// Create mesh
					terrain_comp.CreateMesh(TileSet, SavePath, AssetDatabase);
				}
			}
		}
#endregion
	}
} /*ja2.script*/
