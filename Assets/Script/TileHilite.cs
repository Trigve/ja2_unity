using UnityEngine;
using System.Collections;

public class TileHilite : MonoBehaviour
{
#region Fields
	//! Tile to show.
	public ja2.TerrainTileHandle m_Tile;
	//! Mesh filter.
	private MeshFilter m_MeshFilter;
	//! Terrain manager.
	private ja2.script.TerrainManager m_TerrainManager;
#endregion

#region Properties
	public ja2.TerrainTileHandle tile
	{
		get
		{
			return m_Tile;
		}
		set
		{
			m_Tile = value;
		}
	}
#endregion

#region  Messages
	void Awake()
	{
		// Get components
		m_MeshFilter = GetComponent<MeshFilter>();
		m_TerrainManager = GameObject.Find("TerrainManager").GetComponent<ja2.script.TerrainManager>();
	}
	// Use this for initialization
	void Start ()
	{
		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[4];
		int[] triangles = new int[6];
		Color32[] colors = { new Color32(255, 0, 0, 125), new Color32(255, 0, 0, 125), new Color32(255, 0, 0, 125), new Color32(255, 0, 0, 125) };

		vertices[0] = new Vector3(-ja2.script.TerrainPartition.TILE_HEIGHT, 0, 0);
		vertices[1] = new Vector3(0, 0, -ja2.script.TerrainPartition.TILE_WIDTH);
		vertices[2] = new Vector3(ja2.script.TerrainPartition.TILE_HEIGHT, 0, 0);
		vertices[3] = new Vector3(0, 0, ja2.script.TerrainPartition.TILE_WIDTH);

		triangles[0] = 0;
		triangles[1] = 3;
		triangles[2] = 1;

		triangles[3] = 3;
		triangles[4] = 2;
		triangles[5] = 1;

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors32 = colors;
		m_MeshFilter.mesh = mesh;
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.position = new Vector3(m_TerrainManager.GetPosition(tile, 1).x, 0, m_TerrainManager.GetPosition(tile, 0).z);
	}
#endregion
}
