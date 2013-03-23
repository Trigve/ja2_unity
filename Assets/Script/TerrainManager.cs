using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class TerrainManager : MonoBehaviour
{
#region Constants
	//! Partition name.
	private const string PARTITION_NAME = "Terrain_";
#endregion
#region Attributes
	//! Map instance.
	[SerializeField]
	private MapInstance mapInstance;
	//! Graph.
	[SerializeField]
	private path.Graph graph;
#endregion

#region Properties
	public ja2.Map map { get { return mapInstance.map; } }
#endregion

#region Operations
	//! Get tile position for given tile.
	public Vector3 GetPosition(ja2.TerrainTile Tile, short Vertex)
	{
		// Get the partition used
		int partition_x = Tile.x / ja2.TerrainPartition.PARTITION_WIDTH;
		int partition_y = Tile.y / ja2.TerrainPartition.PARTITION_HEIGHT;
		// Compute normalized terrain tile position
		int normalized_x = Tile.x - ja2.TerrainPartition.PARTITION_WIDTH * partition_x;
		int normalized_y = Tile.y - ja2.TerrainPartition.PARTITION_HEIGHT * partition_y;

		GameObject terrain_go =  GameObject.Find(PARTITION_NAME + partition_x + "_" + partition_y);
		
		return terrain_go.transform.TransformPoint(terrain_go.GetComponent<Terrain>().GetTilePosition(normalized_x, normalized_y, Vertex));
	}

	//! Get position of center of tile.
	public Vector3 GetPosition(ja2.TerrainTile Tile)
	{
		return new Vector3(GetPosition(Tile, 1).x, 0, GetPosition(Tile, 0).z);
	}

	public void CreateTerrain(ja2.Map Map_, ja2.TerrainMaterialManager MatManager)
	{
		if(Map_.width % ja2.TerrainPartition.PARTITION_WIDTH != 0 || Map_.width % ja2.TerrainPartition.PARTITION_WIDTH != 0)
			throw new System.ArgumentException("Map width/height must be normalized to terrain partition width/height.");
		// Create component
		mapInstance = ScriptableObject.CreateInstance<MapInstance>();
		mapInstance.map = Map_;

		// Need to create terrain partitions
		int partition_width = Map_.width / ja2.TerrainPartition.PARTITION_WIDTH;
		int partition_height = Map_.height / ja2.TerrainPartition.PARTITION_HEIGHT;
		for (int i = 0; i < partition_height; ++i)
		{
			for (int j = 0; j < partition_width; ++j)
			{
				// Create terrain GO
				GameObject terrain_go = new GameObject(PARTITION_NAME + j + "_" + i);
				terrain_go.isStatic = true;
				// Set parent
				terrain_go.transform.parent = transform;
				// Update position
				Vector3 tile_vertex_0 = ja2.TerrainPartition.TileVertex(j * ja2.TerrainPartition.PARTITION_WIDTH, i * ja2.TerrainPartition.PARTITION_HEIGHT, 0);
				Vector3 tile_vertex_1 = ja2.TerrainPartition.TileVertex(j * ja2.TerrainPartition.PARTITION_WIDTH, i * ja2.TerrainPartition.PARTITION_HEIGHT, 1);
				terrain_go.transform.position = new Vector3(tile_vertex_0.x, 0, tile_vertex_1.z);
				// Set layer
				terrain_go.layer = Terrain.LAYER;
				// Create component
				var terrain = terrain_go.AddComponent<Terrain>();
				terrain.CreatePartition(j, i, mapInstance, MatManager);
			}
		}
		// Helper set to avoid duplicates
		var duplicate_edges = new System.Collections.Generic.HashSet<string>();
		// Mapping to nodes
		var duplicate_nodes = new System.Collections.Generic.Dictionary<int, int>();
		// Create graph
		var nodes = new System.Collections.Generic.List<int>();
		var edges = new System.Collections.Generic.List<path.Edge>();
		for (int j = 1; j < Map_.height - 1; ++j)
		{
			for (int i = 0; i < Map_.width; ++i)
			{
				// On each odd lines start from 1
				if ((i > 0 && j % 2 == 0) || (j % 2 == 1 && i < Map_.width - 1))
				{
					// Add node
					nodes.Add(Map_.GetTileIndex(i, j));
					duplicate_nodes[Map_.GetTileIndex(i, j)] = nodes.Count - 1;
					// Push edges
					ja2.TerrainTile[] neighbors = Map_.GetAllNeighbors(Map_.GetTile(i, j));
					foreach (ja2.TerrainTile tile in neighbors)
					{
						// Only if valid
						if (tile != null)
						{
							// Isn't duplicate
							string duplicate_name = i + "_" + j + " " + tile.x + "_" + tile.y;
							if (!duplicate_edges.Contains(duplicate_name))
							{
								// Nodes exist
								if (duplicate_nodes.ContainsKey(Map_.GetTileIndex(i, j)) && duplicate_nodes.ContainsKey(Map_.GetTileIndex(tile.x, tile.y)))
								{
									edges.Add(new path.Edge(nodes[duplicate_nodes[Map_.GetTileIndex(i, j)]], nodes[duplicate_nodes[Map_.GetTileIndex(tile.x, tile.y)]]));
									//								edges.Add(new path.Edge(Map_.GetTileIndex(i, j), Map_.GetTileIndex(tile.x, tile.y)));
									duplicate_edges.Add(duplicate_name);
								}
							}
						}
					}
				}
			}
		}
		// Create graph
		graph = new path.Graph(nodes.ToArray(), edges.ToArray(), null);
	}
#endregion
}
