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
#endregion

#region Operations
	public override void Init()
	{
		cameraMain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();
	}

	void FixedUpdate()
	{
		// Camera movement
		if (Input.mousePosition.x <= 20 || Input.GetKey(KeyCode.LeftArrow))
			cameraMain.Move(CameraManager.Direction.LEFT);
		else if (Input.mousePosition.x >= Screen.width - 20 || Input.GetKey(KeyCode.RightArrow))
			cameraMain.Move(CameraManager.Direction.RIGHT);
		else if (Input.mousePosition.y <= 20 || Input.GetKey(KeyCode.DownArrow))
			cameraMain.Move(CameraManager.Direction.BOTTOM);
		else if (Input.mousePosition.y >= Screen.height - 20 || Input.GetKey(KeyCode.UpArrow))
			cameraMain.Move(CameraManager.Direction.TOP);
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