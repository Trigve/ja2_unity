using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Linq;
using UnityEngine;

namespace ja2.script
{
	//! Main class for each level.
	/*!
		In each scene one instance of this component must exist.It handles level
		specific stuff and is served as entry point.
	*/
	public class LevelManager : MonoBehaviourEx
	{
#region Attributes
		//! Game cursor.
		public GameCursor cursor;
		//! Hover cursor.
		public GameObject hover;
		//! Selected mercenary.
		private GameObject soldierSelected;
		//! Path manager.
		public AStarPathManager pathManager;
		//! Soldier path manager.
		private Dictionary<SoldierController, SoldierPathManager> soldiersPaths;
		//! Canceling paths.
		/*!
			This field hold all path for given soldier that are being canceled,
			so they should be finished before any other active path for given
			soldier could run.
		*/
		private Dictionary<SoldierController, SoldierPathManager> m_SoldiersPathsCancelling;
		//! Pending new path.
		/*!
			Pending paths are the one which are being created. We should create
			it only if all canceling paths are done, because we need to find
			actual position of soldier and not the position at which the soldier
			was when new path has-to-be created (because cancelling could change
			the soldier position by one tile).
		*/
		private Dictionary<SoldierController, TerrainTileHandle> m_SoldiersPathsPending;
		//! Path visualizer.
		public PathVisualizer pathVisualizer;
		//! Character definition manager.
		private ja2.CharacterDefinitionManager charDefManager;
		//! Clothes manager.
		private ja2.ClothManager clothManager;
		//! Character entity manager.
		private ja2.CharacterEntityManager charEntityManager;
		//! Debug show of A* path.
		public bool DebugPath;
		//! Terrain manager.
		public TerrainManager m_TerrainManager;
#endregion

#region Properies
		public TerrainManager terrainManager
		{
			get
			{
				return m_TerrainManager;
			}
		}
#endregion

#region Interface
		//! Create full mercenary GO.
		public GameObject CreateSoldier(ja2.Soldier Soldier_)
		{
			// Load prefab of soldier
			var soldier_go = utils.PrefabManager.Create("Soldier");

			var soldier_controller = soldier_go.GetComponent<SoldierController>();
			// Associate terrain
			soldier_controller.terrainManager = terrainManager;
			// Associate solder
			soldier_controller.SetMercenary(Soldier_);

			// Create skinned mesh on parameters and save it
			soldier_go.GetComponent<SoldierController>().combinedMesh = charEntityManager.Create(Soldier_.character(), soldier_go);
			// Activate now, because now is everything set up and we won't get
			// any errors from bones mismatch etc
			soldier_go.SetActive(true);

			return soldier_go;
		}

		//! Update soldier GO.
		public void UpdateSoldier(GameObject SoldierGO)
		{
			var combined_mesh_com = SoldierGO.GetComponent<SoldierController>();
			ja2.Soldier soldier = combined_mesh_com.soldier;
			// Remove old combined mesh
			Destroy(combined_mesh_com.combinedMesh);
			// Create new
			combined_mesh_com.combinedMesh = charEntityManager.Create(soldier.character(), SoldierGO);
			// Must use task here because of unity bug - When mesh is replaced,
			// animation stops to play and is shown in T-pose
			new utils.Task(RebuildCharacterWorkaround(SoldierGO));
		}
#endregion

#region Interface Editor
		//! Create all assets.
		public void CreateAssets(string Path, IEditor AssteDatabase)
		{
			m_TerrainManager.CreateAssets(Path, AssteDatabase);
		}
#endregion

#region Save/Load
		//! Save scene as xml.
		public void SaveSceneXml(XmlWriter Writer, IEditor AssetDatabase)
		{
			// Make the root tag
			Writer.WriteStartElement("level");

			// Serialize map
			m_TerrainManager.SaveXml(Writer, AssetDatabase);

			Writer.WriteEndElement();
		}

		//! Load xml.
		public void LoadXml(XmlReader Reader, IEditor AssetDatabase)
		{
			Reader.MoveToContent();

			m_TerrainManager.LoadXml(Reader, AssetDatabase);
		}

		//! Save scene.
		public void SaveScene(string Path, IEditor AssetDatabase)
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

#region Operations
		//! Bug workaround task for soldier mesh rebuild.
		System.Collections.IEnumerator RebuildCharacterWorkaround(GameObject SoldierGO)
		{
			SoldierGO.SetActive(false);
			yield return new WaitForFixedUpdate();
			SoldierGO.SetActive(true);
		}
#endregion

#region Messages
		protected void Awake()
		{
			soldiersPaths = new Dictionary<SoldierController, SoldierPathManager>();
			m_SoldiersPathsCancelling = new Dictionary<SoldierController, SoldierPathManager>();
			m_SoldiersPathsPending = new Dictionary<SoldierController, TerrainTileHandle>();
			charDefManager = new ja2.CharacterDefinitionManager("Data");
			clothManager = new ja2.ClothManager("Data");
			charEntityManager = new ja2.CharacterEntityManager(charDefManager, clothManager);
		}

		void Update()
		{
			// Update mouse cursor
			cursor.UpdateInfo();
			// Process mercenary selection
			if (Input.GetMouseButtonDown(0))
			{
				GameObject old_selection = soldierSelected;
				// Find all mercenaries
				GameObject[] mercenaries = GameObject.FindGameObjectsWithTag("Mercenary");
				foreach (GameObject mercenary_go in mercenaries)
				{
					// Get component
					var mercenary_ctrl = mercenary_go.GetComponent<SoldierController>();
					if (mercenary_ctrl.position.Equals(cursor.tile))
					{
						// Same selection, do nothing
						if (old_selection == mercenary_go)
						{
							old_selection = null;
							break;
						}
						// Associate new selection
						soldierSelected = mercenary_go;
						// Attach
						hover.transform.parent = mercenary_go.transform;
						hover.transform.localPosition = Vector3.zero;
						break;
					}
				}
				// Wasn't new selection, move
				if (soldierSelected != null && old_selection == soldierSelected)
				{
					var soldier_controller = soldierSelected.GetComponentInChildren<SoldierController>();
					// If there is some path movement in action, cancel it
					if (soldiersPaths.ContainsKey(soldier_controller))
					{
						soldiersPaths[soldier_controller].Cancel();
						// Need to move to cancel dict to be able to finish it
						m_SoldiersPathsCancelling[soldier_controller] = soldiersPaths[soldier_controller];
						soldiersPaths.Remove(soldier_controller);
					}
					// Only if target valid
					if (terrainManager.IsTileWalkable(cursor.tile))
					{
						m_SoldiersPathsPending[soldier_controller] = cursor.tile;
					}
				}
			}
			var paths_cancelled_to_remove = new List<SoldierController>();
			// As first run all canceling paths
			foreach (var it in m_SoldiersPathsCancelling)
			{
				var path_manager = it.Value;
				// Update action
				if (!path_manager.finished)
					path_manager.Run();
				// Has finished, remove it
				else
					paths_cancelled_to_remove.Add(it.Key);
			}
			// Remove all canceled paths now, because we need up to date dict
			paths_cancelled_to_remove.ForEach(Soldier => m_SoldiersPathsCancelling.Remove(Soldier));

			// Create new paths
			var paths_pending_to_remove = new List<SoldierController>();
			foreach (var it in m_SoldiersPathsPending)
			{
				var soldier_controller = it.Key;
				// Be sure that soldier doesn't have any canceling path
				if(!m_SoldiersPathsCancelling.ContainsKey(soldier_controller))
				{
					if (DebugPath)
						pathManager.CreatePathDebug(terrainManager, soldier_controller.position, it.Value);
					else
					{
						var path = pathManager.CreatePath(terrainManager, soldier_controller.position, it.Value);
						// Parse path and create actions
						soldiersPaths[soldier_controller] = new SoldierPathManager(soldier_controller, path);
						// Visualize path
						pathVisualizer.CreatePath(path);
					}
					// Flag for remove
					paths_pending_to_remove.Add(soldier_controller);
				}
			}
			// Remove pending paths
			paths_pending_to_remove.ForEach(Soldier => m_SoldiersPathsPending.Remove(Soldier));

			// Run all active paths
			var paths_to_remove = new List<SoldierController>();
			foreach (var it in soldiersPaths)
			{
				// Update action
				if (!it.Value.finished)
					it.Value.Run();
				// Has finished, remove it
				else
					paths_to_remove.Add(it.Key);
			}
			paths_to_remove.ForEach(Soldier => soldiersPaths.Remove(Soldier));
		}
#endregion
	}
} /*ja2.script*/