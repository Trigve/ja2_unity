using UnityEngine;
using System.Collections;

namespace ja2.script
{
	public class GameCursor : MonoBehaviour
	{
#region Fields
		private RaycastHit m_Hit;
		//! Actual tile cursor is at.
		private ja2.TerrainTileHandle m_Tile;
		//! Last mouse position.
		private Vector3 m_LastMousePos;
		public TerrainManager m_TerrainManager;
#endregion

#region Properties
		public ja2.TerrainTileHandle tile { get { return m_Tile; } }
#endregion

#region Messages
		void Awake()
		{
			var mesh_filter = GetComponent<MeshFilter>();
			// Draw lines
			Mesh mesh = new Mesh();
			// Create vertex array
			Vector3[] array_vec = new Vector3[20];

			Vector3 up_vec = new Vector3(0, 2, 0);
			// Make the vertices

			// Down
			array_vec[0] = new Vector3(-TerrainPartition.TILE_HEIGHT, 0, 0);
			array_vec[1] = new Vector3(0, 0, -TerrainPartition.TILE_WIDTH);
			array_vec[2] = new Vector3(TerrainPartition.TILE_HEIGHT, 0, 0);
			array_vec[3] = new Vector3(0, 0, TerrainPartition.TILE_WIDTH);
			// Upd
			array_vec[4] = array_vec[0] + up_vec;
			array_vec[5] = array_vec[1] + up_vec;
			array_vec[6] = array_vec[2] + up_vec;
			array_vec[7] = array_vec[3] + up_vec;

			array_vec[8] = array_vec[0];
			array_vec[9] = array_vec[1];
			array_vec[10] = array_vec[5];
			array_vec[11] = array_vec[4];

			array_vec[12] = array_vec[1];
			array_vec[13] = array_vec[2];
			array_vec[14] = array_vec[6];
			array_vec[15] = array_vec[5];

			array_vec[16] = array_vec[2];
			array_vec[17] = array_vec[3];
			array_vec[18] = array_vec[7];
			array_vec[19] = array_vec[6];

			int[] array_ind = 
		{
			0, 1,
			1, 2,
			2, 3,
			3, 0,

			4, 5,
			5, 6,
			6, 7,
			7, 4,

			8, 9,
			9, 10,
			10, 11,
			11, 8,
 
			12, 13,
			13, 14,
			14, 15,
			15, 12,

			16, 17,
			17, 18,
			18, 19,
			19, 16
		};

			mesh.vertices = array_vec;
			mesh.SetIndices(array_ind, MeshTopology.Lines, 0);

			mesh_filter.mesh = mesh;
		}

		// Use this for initialization
		void Start()
		{
			Screen.showCursor = false;
		}

		// Update is called once per frame
		void Update()
		{
			// Update position only if mouse was moved
			if (m_LastMousePos != Input.mousePosition)
			{
				m_LastMousePos = Input.mousePosition;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out m_Hit, Mathf.Infinity, TerrainPartition.LAYER_MASK))
				{
					// Find the tile based on triangles			
					TerrainPartition terrain = m_Hit.transform.gameObject.GetComponent<TerrainPartition>();
					m_Tile = terrain.GetTileHandle(m_Hit.triangleIndex);

					// Get center of tile
					transform.position = terrain.GetCenterOfTile(m_Hit.triangleIndex);
				}
			}
		}
#endregion
	}
} /*ja2.script*/