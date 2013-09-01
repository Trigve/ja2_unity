using UnityEngine;
using System.Collections;
using System;

namespace ja2
{
	[Serializable]
	sealed public class TerrainTile : ScriptableObject
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

		[Flags]
		public enum Type
		{
			NONE = 0,
			REGULAR = 1,
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
		//! Terrain type.
		[SerializeField]
		private Type type_;
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

		//! Is tile walkable.
		public bool walkable()
		{
			return ((type_ & Type.REGULAR) != 0);
		}
#endregion

#region Construction
		public static TerrainTile Construct(int X, int Y, Type Type_)
		{
			var obj = CreateInstance<TerrainTile>();
			obj.m_x = X;
			obj.m_y = Y;
			obj.terrainTypeArray = new byte[4];
			obj.type_ = Type_;

			return obj;
		}
#endregion
	}
}
