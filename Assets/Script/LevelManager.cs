using UnityEngine;
using System.Collections.Generic;

//! Main class for each level.
/*!
	In each scene one instance of this component must exist.It handles level
	specific stuff and is served as entry point.
*/
public class LevelManager : MonoBehaviourEx
{
#region Attributes
	//! Game cursor.
	private GameCursor cursor;
	//! Hover cursor.
	private GameObject hover;
	//! Selected mercenary.
	private GameObject soldierSelected;
	//! Terrain manager.
	private TerrainManager terrainManager;
	//! Path manager.
	private AStarPathManager pathManager;
	//! Soldier path manager.
	private Dictionary<SoldierController, SoldierPathManager> soldiersPaths;
	//! Path visualizer.
	private PathVisualizer pathVisualizer;
	//! Character definition manager.
	private ja2.CharacterDefinitionManager charDefManager;
	//! Clothes manager.
	private ja2.ClothManager clothManager;
	//! Character entity manager.
	private CharacterEntityManager charEntityManager;
#endregion

#region Operations
	void Awake()
	{
		terrainManager = GameObject.Find("Map").GetComponent<TerrainManager>();
		// Create A* path manager and GO
		pathManager = PrefabManager.Create("AStartPathManager").GetComponent<AStarPathManager>();
		pathManager.transform.parent = transform;

		soldiersPaths = new Dictionary<SoldierController, SoldierPathManager>();
		// Instantiate cursor if not found
		GameObject cursor_go;
		if ((cursor_go = GameObject.Find("Cursor")) == null)
		{
			var prefab_class = Resources.Load("Prefabs/Cursor", typeof(GameObject));
			cursor_go = (GameObject)Instantiate(prefab_class);
			cursor_go.name = prefab_class.name;
			// Save it
			cursor = cursor_go.GetComponent<GameCursor>();
		}
		// Instantiate hover
		if ((hover = GameObject.Find("Hover")) == null)
		{
			var prefab_class = Resources.Load("Prefabs/Hover", typeof(GameObject));
			hover = (GameObject)Instantiate(prefab_class);
			hover.name = prefab_class.name;
		}
		// Create path visulizer child GO and get main component
		var path_visualizer_go = PrefabManager.Create("PathVisualizer");
		path_visualizer_go.transform.parent = transform;
		pathVisualizer = path_visualizer_go.GetComponent<PathVisualizer>();

		charDefManager = new ja2.CharacterDefinitionManager(Application.dataPath + "/Resources/Data");
		clothManager = new ja2.ClothManager(Application.dataPath + "/Resources/Data");
		charEntityManager = new CharacterEntityManager(charDefManager, clothManager);
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
				if (mercenary_ctrl.position == cursor.tile)
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
				if (cursor.tile.walkable())
				{
					// Search for path
					var soldier_controller = soldierSelected.GetComponentInChildren<SoldierController>();
					var path = pathManager.CreatePath(terrainManager, soldier_controller.position, cursor.tile);
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
		foreach(var it in paths_to_remove)
		{
			soldiersPaths.Remove(it);
		}
	}
#endregion
}
