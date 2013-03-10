using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class TerrainManager : MonoBehaviour
{
#region Attributes
	//! Map instance.
	[SerializeField]
	private MapInstance mapInstance;
	//! All meshes.
	private Mesh[] m_Meshes;
#endregion

#region Operations
	// Helper for now. Should be replaced with partiotions
	public ja2.TerrainTile GetTile(int TriangleIndex, GameObject Object)
	{
		return Object.GetComponentInChildren<Terrain>().GetTile(TriangleIndex);
	}

	public void CreateTerrain(ja2.Map Map_, ja2.TerrainMaterialManager MatManager)
	{
		mapInstance = ScriptableObject.CreateInstance<MapInstance>();
		mapInstance.map = Map_;
		// Add terrain GO
		GameObject terrain_go = new GameObject("Terrain_0");
		// Set parent
		terrain_go.transform.parent = transform;
		// Set layer
		terrain_go.layer = Terrain.LAYER;
		// Create component
		terrain_go.AddComponent<MeshFilter>();
		terrain_go.AddComponent<MeshRenderer>();
		var terrain = terrain_go.AddComponent<Terrain>();
		terrain.CreateMap(mapInstance, MatManager);
	}
#endregion
}
