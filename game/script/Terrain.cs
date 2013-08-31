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
		Mesh mesh = terrainPartition.Create(X, Y, Map.map, tile_set);
		// Add needed components
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
		var mesh_renderer = gameObject.GetComponent<MeshRenderer>();
		// Set map material
		mesh_renderer.sharedMaterial = Resources.LoadAssetAtPath("Assets/Materials/" + tile_set.materialName + ".mat", typeof(Material)) as Material;
		// Don't cast shadows
		mesh_renderer.castShadows = false;
	}
	//! Get tile for given triangle.
	public ja2.TerrainPartition.TriangleMap GetTile(int Triangle)
	{
		return terrainPartition.GetTile(Triangle);
	}
	//! Get tile position.
	public Vector3 GetTilePosition(int X, int Y, short Vertex)
	{
		return TerrainPartition.TileVertex(X, Y, Vertex);
	}
	#endregion
}
