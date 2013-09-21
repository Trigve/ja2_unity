using System;
using UnityEngine;

namespace ja2
{
	//! Non moveable object.
	[Serializable]
	public sealed class NonMoveableObject
	{
#region Fields
		//! ID.
		private Guid m_Id;
		//! Tile associated to object.
		private TerrainTile m_Tile;
#endregion

#region Operations
		//! Get handle.
		public NonMoveableObjectHandle GetHandle()
		{
			return new NonMoveableObjectHandle(m_Id.ToString());
		}

		//! Get id.
		public Guid id
		{
			get
			{
				return m_Id;
			}
		}
#endregion
#region Construction
		public NonMoveableObject(Guid Id, TerrainTile Tile)
		{
			m_Id = Id;
			m_Tile = Tile;
		}
#endregion
	}
} /*ja2*/
