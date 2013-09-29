using UnityEngine;
using System.Collections;
using System;
using System.Xml;

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

#region Fields
		//! X.
		private int m_x;
		//! Y.
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
		private byte[] terrainTypeArray;
		//! Terrain type.
		private Type type_;
#endregion

#region Properties
		public int x { get { return m_x; } private set { m_x = value; } }
		public int y { get { return m_y; } private set { m_y = value; } }
#endregion

#region Save/Load
		//! Save to xml.
		public void SaveXml(XmlWriter Writer)
		{
			Writer.WriteStartElement("tile");

			Writer.WriteAttributeString("x", m_x.ToString());
			Writer.WriteAttributeString("y", m_y.ToString());
			Writer.WriteAttributeString("variant", variant.ToString());

			Writer.WriteAttributeString("type_0", terrainTypeArray[0].ToString());
			Writer.WriteAttributeString("type_1", terrainTypeArray[1].ToString());
			Writer.WriteAttributeString("type_2", terrainTypeArray[2].ToString());
			Writer.WriteAttributeString("type_3", terrainTypeArray[3].ToString());

			Writer.WriteAttributeString("type", ((ushort)type_).ToString());

			Writer.WriteEndElement();
		}
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
		public TerrainTile(int X, int Y, Type Type_)
		{
			x = X;
			y = Y;
			terrainTypeArray = new byte[4];
			type_ = Type_;
		}
#endregion
	}
}
