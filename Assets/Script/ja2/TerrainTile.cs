using UnityEngine;
using System.Collections;
using System;

namespace ja2
{
	[Serializable]
	sealed public class TerrainTile
	{
#region Enums
		//! Vertex numbers.
		public enum Vertex
		{
			NORTH = 0,
			WEST = 1,
			SOUTH = 2,
			EAST = 3,
		}
#endregion
		#region Constants
		private const byte VERTEX_COUNT = 4;
		#endregion

#region Attributes
		//! X.
		[SerializeField]
		private int m_x;
		//! Y.
		[SerializeField]
		private int m_y;
		//! Type variant index.
		/*!
			This index is used to use different terrain type variant.
		*/
		public byte variant;
		//! Each vertex's terrain type (grass, mud, ...).
		/*!
			Maximum of 2 different type per tile could be specified which will be blended together.
		*/
		[SerializeField]
		private byte[] terrainTypeArray;
#endregion

#region Properties
		public int x { get { return m_x; } private set { m_x = value; } }
		public int y { get { return m_y; } private set { m_y = value; } }
#endregion

#region Operations
		//! Get terrain type.
		public byte GetTerrainType(Vertex Vertex_)
		{
			return terrainTypeArray[(int)Vertex_];
		}
		//! Set terrain type.
		public void SetTerrainType(Vertex Vertex_, byte Type)
		{
			terrainTypeArray[(int)Vertex_] = Type;
		}
		#endregion

		#region Construction
		public TerrainTile(int X, int Y)
		{
			x = X;
			y = Y;
			terrainTypeArray = new byte[4];
		}
#endregion
	}
}
