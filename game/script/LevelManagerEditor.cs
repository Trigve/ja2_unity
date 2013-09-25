using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ja2.script
{
	//! Level manager for use in editor.
	public sealed class LevelManagerEditor : LevelManager
	{
#region Fields
		//! Terrain manager.
		public TerrainManagerEditor m_TerrainManager;
#endregion

#region Properties
		public override TerrainManager terrainManager
		{
			get
			{
				return m_TerrainManager;
			}
		}

		public TerrainManagerEditor terrainManagerEditor
		{
			get
			{
				return m_TerrainManager;
			}
		}
#endregion

#region Save/Load
		//! Save scene.
		public void SaveScene(string Path, IAssetDatabase AssetDatabase)
		{
			// Testing for now
			var stream = new FileStream(Application.dataPath + "/Resources/Scenes/" + Path + "scene.bytes", FileMode.Create);
			var formatter = new BinaryFormatter();

			// Serialize map
			m_TerrainManager.Save(formatter, stream, AssetDatabase);

			stream.Flush();
			stream.Close();
		}
#endregion
	}
} /*ja2.script*/
