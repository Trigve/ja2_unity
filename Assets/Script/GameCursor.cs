using UnityEngine;
using System.Collections;

public class GameCursor : MonoBehaviour
{
#region Attributes
	private RaycastHit m_Hit;
	private ja2.Map map;
#endregion
	void Awake()
	{
		var mesh_filter = GetComponent<MeshFilter>();
		// Draw lines
		Mesh mesh = new Mesh();
		// Create vertex array
		Vector3[] array_vec = new Vector3[20];
		// Create triangles array
		Vector2[] uv = new Vector2[array_vec.Length];
		
		Vector3 up_vec = new Vector3(0, 2, 0);
		// Make the vertices
		array_vec[0] = new Vector3(-Terrain.TILE_HEIGHT, 0, 0);
		uv[0] = new Vector2(1, 0);
		array_vec[1] = new Vector3(0, 0, -Terrain.TILE_WIDTH);
		uv[1] = new Vector2(0, 0);
		array_vec[2] = new Vector3(Terrain.TILE_HEIGHT, 0, 0);
		uv[2] = new Vector2(0, 1);
		array_vec[3] = new Vector3(0, 0, Terrain.TILE_WIDTH);
		uv[3] = new Vector2(1, 1);

		array_vec[4] = array_vec[0] + up_vec;
		uv[4] = uv[0];
		array_vec[5] = array_vec[1] + up_vec;
		uv[5] = uv[1];
		array_vec[6] = array_vec[2] + up_vec;
		uv[6] = uv[2];
		array_vec[7] = array_vec[3] + up_vec;
		uv[7] = uv[3];

		array_vec[8] = array_vec[0];
		uv[8] = new Vector2(1, 1);
		array_vec[9] = array_vec[1];
		uv[9] = new Vector2(0, 1);
		array_vec[10] = array_vec[5];
		uv[10] = new Vector2(0, 0);
		array_vec[11] = array_vec[4];
		uv[11] = new Vector2(1, 0);

		array_vec[12] = array_vec[1];
		uv[12] = new Vector2(0, 1);
		array_vec[13] = array_vec[2];
		uv[13] = new Vector2(1, 1);
		array_vec[14] = array_vec[6];
		uv[14] = new Vector2(1, 0);
		array_vec[15] = array_vec[5];
		uv[15] = new Vector2(0, 0);

		array_vec[16] = array_vec[2];
		uv[16] = new Vector2(0, 1);
		array_vec[17] = array_vec[3];
		uv[17] = new Vector2(1, 1);
		array_vec[18] = array_vec[7];
		uv[18] = new Vector2(1, 0);
		array_vec[19] = array_vec[6];
		uv[19] = new Vector2(0, 0);
/*
		array_vec[20] = array_vec[3];
		uv[20] = new Vector2(0.5f, 0);
		array_vec[21] = array_vec[0];
		uv[21] = new Vector2(0, 0.5f);
		array_vec[22] = array_vec[4];
		uv[22] = new Vector2(0.5f, 1);
		array_vec[23] = array_vec[7];
		uv[23] = new Vector2(1, 0.5f);
*/

		// Triangles
		int[] array_tri = 
		{
			0, 3, 1,
			1, 3, 2,
			
			4, 7, 5,
			5, 7, 6,

			10, 8, 9,
			10, 11, 8,
 
			13, 12, 15,
			15, 14, 13,

			17, 16, 19,
			18, 17, 19
/*
			21, 20, 23,
			22, 21, 23
*/
		};

		mesh.vertices = array_vec;
		mesh.triangles = array_tri;
		mesh.uv = uv;
		mesh.RecalculateNormals();
		

		mesh_filter.mesh = mesh;
		mesh_filter.renderer.sharedMaterial.renderQueue = 3000;
	}

	// Use this for initialization
	void Start ()
	{
#if !UNITY_EDITOR
		Screen.showCursor = false;
#endif
		map = GameObject.Find("Map").GetComponent<TerrainManager>().map;
	}
	
	// Update is called once per frame
	void Update ()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out m_Hit, Mathf.Infinity, Terrain.LAYER_MASK))
		{
			// Find the tile based on triangles			
			Terrain terrain = m_Hit.transform.gameObject.GetComponent<Terrain>();
			ja2.TerrainPartition.TriangleMap tile_x_y = terrain.GetTile(m_Hit.triangleIndex);
			ja2.TerrainTile tile = map.GetTile(tile_x_y.x, tile_x_y.y);

			Vector3 v0 = ja2.TerrainPartition.TileVertex(tile.x, tile.y, 0);
			Vector3 v1 = ja2.TerrainPartition.TileVertex(tile.x, tile.y, 1);
			transform.position = new Vector3(v1.x, 0, v0.z);
		}
	}
}
