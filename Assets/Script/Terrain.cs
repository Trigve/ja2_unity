using UnityEngine;
using System.Collections;
using System;
using ja2;

[RequireComponent (typeof (MeshFilter), typeof (MeshRenderer), typeof (MeshCollider))]
public sealed class Terrain : MonoBehaviour
{

	//! Tile width.
	public const float TILE_WIDTH = 0.7071067F;
	//! Tile width.
	public const float TILE_HEIGHT = TILE_WIDTH;
	//! Layer index.
	public const int LAYER = 8;
	//! Layer mask
	public const int LAYER_MASK = 1 << 8;
	//	public TerrainGen terrainGen {get; private set;}
	[SerializeField, HideInInspector]
	private ja2.TerrainPartition terrainPartition;

#region Operations
	public void CreatePartition(int X, int Y, MapInstance Map, TerrainMaterialManager MatManager)
	{
		TerrainTileSet tile_set = MatManager.GetTerrainSet(Map.map.terrainName);
		// Create terrain partition
		terrainPartition = new ja2.TerrainPartition();
		Mesh mesh = terrainPartition.Create(X, Y, Map, tile_set);
		// Add needed components
		GetComponent<MeshFilter>().mesh = mesh;
		gameObject.getComponent<MeshCollider>().sharedMesh = mesh;
		var mesh_renderer = gameObject.GetComponent<MeshRenderer>();
		// Set map material
		
		mesh_renderer.sharedMaterial = Resources.Load("Materials/" + tile_set.materialName, typeof(Material)) as Material;
	}

	//! Get tile for given triangle.
	public ja2.TerrainPartition.TriangleMap GetTile(int Triangle)
	{
		return terrainPartition.GetTile(Triangle);
	}
	#endregion
}
