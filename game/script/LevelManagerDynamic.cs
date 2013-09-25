using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;

namespace ja2.script
{
	//! Level manager for dynamic levels.
	public class LevelManagerDynamic : LevelManager
	{
#region Fields
		//! Terrain manager.
		public TerrainManager m_TerrainManager;
#endregion

#region Properties
		public override TerrainManager terrainManager
		{
			get
			{
				return m_TerrainManager;
			}
		}
#endregion

#region Messages
		new void Awake()
		{
			// Load all data
			var level_data = (TextAsset)Resources.Load("Scenes/1/scene", typeof(TextAsset));

			var formatter = new BinaryFormatter();
			var stream = new MemoryStream(level_data.bytes);

			// Map
			terrainManager.Load(formatter, stream);
			
			stream.Close();

			base.Awake();
		}
#endregion
	}
} /*ja2.script*/
