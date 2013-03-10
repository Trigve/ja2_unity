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
	public int a;

	public override void Init()
	{
		// Load data from save-game
		a = 5;
	}
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