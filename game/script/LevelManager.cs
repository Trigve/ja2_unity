using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEditor;

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
		//! Terrain manager.
		public TerrainManager m_TerrainManager;
		//! Path manager.
		public AStarPathManager pathManager;
		//! Soldier path manager.
		private Dictionary<SoldierController, SoldierPathManager> soldiersPaths;
		//! Path visualizer.
		public PathVisualizer pathVisualizer;
		//! Character definition manager.
		private ja2.CharacterDefinitionManager charDefManager;
		//! Clothes manager.
		private ja2.ClothManager clothManager;
		//! Character entity manager.
		private ja2.CharacterEntityManager charEntityManager;
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
		//! Save scene.
		public void SaveScene(string Path)
		{
			// Testing for now
			var stream = new FileStream(Application.dataPath + "/Resources/Scenes/" + Path + "scene.bytes", FileMode.Create);
			var formatter = new BinaryFormatter();

			// Serialize map
			m_TerrainManager.Save(formatter, stream);

			stream.Flush();
			stream.Close();

			AssetDatabase.Refresh();
		}

		//! Create full mercenary GO.
		public GameObject CreateSoldier(ja2.Soldier Soldier_)
		{
			// Load prefab of soldier
			var soldier_go = utils.PrefabManager.Create("Soldier");

			var soldier_controller = soldier_go.GetComponent<SoldierController>();
			// Associate terrain
			soldier_controller.terrainManager = m_TerrainManager;
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
		public void UpdateSoldier(ja2.Soldier Soldier_, GameObject SoldierGO)
		{

			var combined_mesh_com = SoldierGO.GetComponent<SoldierController>();
			// Remove old combined mesh
			Destroy(combined_mesh_com.combinedMesh);
			// Create new
			combined_mesh_com.combinedMesh = charEntityManager.Create(Soldier_.character(), SoldierGO);
			// Must use task here because of unity bug - When mesh is replaced,
			// animation stops to play and is shown in T-pose
			new utils.Task(RebuildCharacterWorkaround(SoldierGO));
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
			charDefManager = new ja2.CharacterDefinitionManager("Data");
			clothManager = new ja2.ClothManager("Data");
			charEntityManager = new ja2.CharacterEntityManager(charDefManager, clothManager);
		}

		void Update()
		{
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
					// Only if target valid
					if (m_TerrainManager.GetTile(cursor.tile).walkable())
					{
						// Search for path
						var soldier_controller = soldierSelected.GetComponentInChildren<SoldierController>();
						var path = pathManager.CreatePath(m_TerrainManager, soldier_controller.position, cursor.tile);
						// Parse path and create actions
						soldiersPaths[soldier_controller] = new SoldierPathManager(soldier_controller, path);
						// Visualize path
						pathVisualizer.CreatePath(path);
					}
				}
			}
			// Run all paths
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
			// Remove unneeded one
			foreach (var it in paths_to_remove)
			{
				soldiersPaths.Remove(it);
			}
		}
#endregion
	}
} /*ja2.script*/