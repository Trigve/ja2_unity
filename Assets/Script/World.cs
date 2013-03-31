using UnityEngine;
using System.Collections;
using System;

/*
public sealed class World : Singleton<World>
{
	public int a;
}
*/

public sealed class World : MonoSingleton<World>
{
#region Attributes
	//! Main camera.
	private CameraManager cameraMain;
	//! Game cursor.
	private GameCursor cursor;
	//! Hover cursor.
	private GameObject hover;
	//! Selected mercenary.
	private GameObject soldierSelected;
#endregion

#region Operations
	public override void Init()
	{
		cameraMain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();
	}

	void Awake()
	{
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
	}

	void Start()
	{
	}

	void FixedUpdate()
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
				var mercenary_ctrl = mercenary_go.GetComponentInChildren<SoldierController>();
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
		}
	}


#endregion
	
}

/*
//! World class
[Serializable]
public sealed class World : ScriptableObject
{
	public int a;
	//! Static instance.
	private static World m_Instance;

	//! Accessor for outer world.
	public static World Instance
	{
		get
		{
			// Not found, create
			if (m_Instance == null)
			{
				m_Instance = ScriptableObject.CreateInstance<World>();
				m_Instance.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_Instance;
		}
	}

#region Attributes
#endregion

#region Operations
	public void OnEnabled()
	{
		hideFlags = HideFlags.HideAndDontSave;
	}
	public void OnApplicationQuit()
	{
		m_Instance = null;
	}
#endregion

	void Awake()
	{
		// Don't destroy object
//		DontDestroyOnLoad(gameObject);
	}

}
*/