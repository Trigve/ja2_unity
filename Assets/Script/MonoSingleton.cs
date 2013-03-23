using UnityEngine;
using System.Collections;
using System;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
	private static T m_Instance = null;
	public static T instance
	{
		get
		{
			// Instance requiered for the first time, we look for it
			if (m_Instance == null)
			{
				m_Instance = GameObject.FindObjectOfType(typeof(T)) as T;

				// Object not found, we create a temporary one
				if (m_Instance == null)
				{
					Debug.LogWarning("No instance of " + typeof(T).ToString() + ", a temporary one is created.");
					var go = new GameObject(typeof(T).ToString(), typeof(T));
					// Don't show

					
					m_Instance = go.GetComponent<T>();
					// Problem during the creation, this should not happen
					if (m_Instance == null)
					{
						Debug.LogError("Problem during the creation of " + typeof(T).ToString());
					}
				}

				m_Instance.Init();
			}
			return m_Instance;
		}
	}
	// If no other monobehaviour request the instance in an awake function
	// executing before this one, no need to search the object.
	private void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = this as T;
			m_Instance.Init();
		}
	}

	// This function is called when the instance is used the first time
	// Put all the initializations you need here, as you would do in Awake
	public virtual void Init() { }
/*
	public void OnDisable()
	{
#if UNITY_EDITOR
		if (Application.isEditor)
		{
			if (instance != null)
			{
				// Object.Destroy is delayed, and never completes in the editor, so use DestroyImmediate instead.
				UnityEngine.Object.Destroy(instance.gameObject);
			}
		}
#endif
	}
*/
	// Make sure the instance isn't referenced anymore when the user quit, just in case.
	private void OnApplicationQuit()
	{
		m_Instance = null;
	}
}
