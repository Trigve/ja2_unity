using UnityEngine;
using System.Collections;

namespace ja2.script
{
	//! Cursor implementation.
	/*!
		Default cursor is 3D, but if tile is non-moveable, show the mouse cursor
		with custom texture.
	*/
	public class GameCursor : MonoBehaviour
	{
#region Fields
		//! Terrain manager to use.
		public TerrainManager m_TerrainManager;
		//! 3D cursor material.
		public Material m_Material3D;
		//! 2D cursor texture.
		public Texture2D m_TextureNonMoveable;
		private RaycastHit m_Hit;
		//! Actual tile cursor is at.
		private ja2.TerrainTileHandle m_Tile;
		//! Last mouse position.
		private Vector3 m_LastMousePos;
		//! Mesh for 3D cursor.
		private Mesh m_Mesh3D;
		//! Cached mesh filter.
		private MeshFilter m_MeshFilter;
		//! Cached mesh renderer.
		private MeshRenderer m_MeshRenderer;
#endregion

#region Properties
		public ja2.TerrainTileHandle tile { get { return m_Tile; } }
#endregion

#region Interface
		//! Update the cursor info.
		public void UpdateInfo()
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
					// Change cursor if needed
					if (terrain.IsWalkable(m_Tile.x, m_Tile.y))
					{
						// Change to 3D cursor
						if (m_MeshFilter.sharedMesh != m_Mesh3D)
						{
							Screen.showCursor = false;
							Cursor.SetCursor(null, new Vector2(), CursorMode.Auto);

							m_MeshFilter.sharedMesh = m_Mesh3D;
							m_MeshRenderer.sharedMaterial = m_Material3D;
						}
					}
					else
					{
						if (m_MeshFilter.sharedMesh == m_Mesh3D)
						{
							m_MeshFilter.sharedMesh = null;
							Screen.showCursor = true;
							Cursor.SetCursor(m_TextureNonMoveable, new Vector2(32, 32), CursorMode.Auto);
						}
					}
				}
			}
		}
#endregion
#region Messages
		void Awake()
		{
			m_MeshFilter = GetComponent<MeshFilter>();
			m_MeshRenderer = GetComponent<MeshRenderer>();

			{
				// Draw lines
				m_Mesh3D = new Mesh();
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

				m_Mesh3D.vertices = array_vec;
				m_Mesh3D.SetIndices(array_ind, MeshTopology.Lines, 0);
			}

			// Default is 3d
			m_MeshFilter.sharedMesh = m_Mesh3D;
			m_MeshRenderer.sharedMaterial = m_Material3D;
		}

		// Use this for initialization
		void Start()
		{
			Screen.showCursor = false;
		}
#endregion
	}
} /*ja2.script*/