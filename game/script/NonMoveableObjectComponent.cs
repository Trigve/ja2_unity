using System;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ja2.script
{
	//! Component for non moveable object
	[Serializable]
	public class NonMoveableObjectComponent : MonoBehaviourEx
	{
#region Fields
		//! Terrain partition.
		[SerializeField]
		private TerrainPartition m_Parent;
		//! Instance of non-moveable.
		private ja2.NonMoveableObject m_Instance;
#endregion

#region Properties

		public ja2.NonMoveableObject nonMoveable
		{
			get
			{
				return m_Instance;
			}
		}

		//! Set parent.
		public TerrainPartition parent
		{
			get
			{
				return m_Parent;
			}
			set
			{
				m_Parent = value;
			}
		}
#endregion

#region Save/Load
		//! Save xml.
		public void SaveXml(XmlWriter Writer, IAssetDatabase AssetDatabase)
		{
			Writer.WriteStartElement("item");
			// Write original prefab
			Writer.WriteAttributeString("prefab", AssetDatabase.GetAssetPath(AssetDatabase.GetPrefabParent(gameObject)));
			// Save instance
			m_Instance.SaveXml(Writer);
			Writer.WriteEndElement();
		}

		public void Save(IFormatter Formatter, Stream Stream_)
		{
			Formatter.Serialize(Stream_, m_Instance);
		}

		public void Load(IFormatter Formatter, Stream Stream_)
		{
			m_Instance = (NonMoveableObject)Formatter.Deserialize(Stream_);
		}
#endregion

#region Messages
#endregion

#region Construction
		public void This()
		{
			m_Instance = new ja2.NonMoveableObject();
		}
#endregion
	}
} /*j2.script*/
