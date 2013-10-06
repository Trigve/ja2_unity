using System;
using UnityEngine;

namespace ja2.script
{
	//! 3D cursor implementation.
	public class GameCursor3D : MonoBehaviourEx, IGameCursor
	{
#region Fields
		//! 3D cursor material.
		public Material m_Material3D;
		//! Cached mesh filter.
		private MeshFilter m_MeshFilter;
		//! Cached mesh renderer.
		private MeshRenderer m_MeshRenderer;
		//! Mesh for 3D selection cursor.
		private Mesh m_Mesh3DSelect;
#endregion


#region Interface
		//! Set the cursor type.
		public void SetCursorType(GameCursorType CursorType)
		{
			switch(CursorType)
			{
				case GameCursorType.Cursor3D:
					m_MeshFilter.sharedMesh = m_Mesh3DSelect;
					m_MeshRenderer.sharedMaterial = m_Material3D;
					break;
			}
		}
#endregion
#region Messages
		void Awake()
		{
			// Draw lines
			m_Mesh3DSelect = new Mesh();
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

			m_Mesh3DSelect.vertices = array_vec;
			m_Mesh3DSelect.SetIndices(array_ind, MeshTopology.Lines, 0);

			m_MeshFilter = GetComponent<MeshFilter>();
			m_MeshRenderer = GetComponent<MeshRenderer>();

			// Set the mesh
			m_MeshFilter.sharedMesh = m_Mesh3DSelect;
			m_MeshRenderer.sharedMaterial = m_Material3D;
		}
#endregion
	}
} /*ja2.script*/