using System;
using UnityEngine;

namespace ja2
{
	//! Property for tile.
	[Serializable]
	public sealed class TerrainTileProperty
	{
#region Fields
		//! Non-moveable object handle associated.
		private NonMoveableObjectHandle m_NonMoveableObject;
#endregion

#region Properties
		public NonMoveableObjectHandle nonMoveable
		{
			get
			{
				return m_NonMoveableObject;
			}
			set
			{
				m_NonMoveableObject = value;
			}
		}
#endregion

#region Operations
#endregion

#region Construction
		public TerrainTileProperty()
		{
		}
#endregion
	}
} /*ja2*/
