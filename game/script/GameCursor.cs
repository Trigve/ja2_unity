using System;
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
		//! 3D cursor.
		public GameCursor3D m_Cursor3D;
		//! 2D cursor.
		public GameCursor2D m_Cursor2D;
		//! Actual tile cursor is at.
		private ja2.TerrainTileHandle m_Tile;
		//! Last mouse position.
		private Vector3 m_LastMousePos;
		//! Actual cursor type.
		private ja2.GameCursorType m_ActualCursor;
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
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, TerrainPartition.LAYER_MASK))
				{
					// Find the tile based on triangles			
					TerrainPartition terrain = hit.transform.gameObject.GetComponent<TerrainPartition>();
					m_Tile = terrain.GetTileHandle(hit.triangleIndex);
					// Get center of tile
					transform.position = terrain.GetCenterOfTile(hit.triangleIndex);

					// Old cursor
					GameCursorType old_cursor_type = m_ActualCursor;
					// Change cursor if needed
					if (terrain.IsWalkable(m_Tile.x, m_Tile.y))
						m_ActualCursor = GameCursorType.Cursor3D;
					else
						m_ActualCursor = GameCursorType.Nonmoveable;

					// Only if cursor changed
					if(old_cursor_type != m_ActualCursor)
						UpdateActiveCursor();
				}
			}
		}
#endregion

#region Operations
		//! Enable 3D cursor.
		private IGameCursor Enable3D()
		{
			if (!m_Cursor3D.gameObject.activeSelf)
				m_Cursor3D.gameObject.SetActive(true);
			if(m_Cursor2D.gameObject.activeSelf)
				m_Cursor2D.gameObject.SetActive(false);

			return m_Cursor3D;
		}

		//! Enable 2D cursor.
		private IGameCursor Enable2D()
		{
			if (!m_Cursor2D.gameObject.activeSelf)
				m_Cursor2D.gameObject.SetActive(true);
			if(m_Cursor3D.gameObject.activeSelf)
				m_Cursor3D.gameObject.SetActive(false);

			return m_Cursor2D;
		}

		//! Update the active cursor.
		private void UpdateActiveCursor()
		{
			IGameCursor active_cursor = null;

			switch (m_ActualCursor)
			{
				case GameCursorType.Cursor3D:
					active_cursor = Enable3D();
					break;
				case GameCursorType.Nonmoveable:
					active_cursor = Enable2D();
					break;
			}

			// Set the cursor type
			active_cursor.SetCursorType(m_ActualCursor);
		}
#endregion
#region Messages
		// Use this for initialization
		void Start()
		{
			Screen.showCursor = false;

			m_ActualCursor = GameCursorType.Cursor3D;

			UpdateActiveCursor();
		}
#endregion
	}
} /*ja2.script*/