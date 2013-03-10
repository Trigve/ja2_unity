using UnityEngine;
using System;
using System.Collections.Generic;
/*
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour, new()
{

	private static T m_Instance = default(T);
	
	public static T instance
	{
		get
		{
			// Instance required for the first time, we look for it
			if (m_Instance == null)
			{
				string NAME = "Singleton<" + typeof(T).ToString() + ">";
				var go = GameObject.Find(NAME);
				if (go == null)
				{
					go = new GameObject(NAME);
//					if (!Application.isPlaying)
//						go.hideFlags = HideFlags.HideAndDontSave;
				}
				else
				{
//					if (Application.isPlaying)
//						go.hideFlags = 0;
				}
				// Find component
				m_Instance = go.GetComponent<T>();
				if (m_Instance == null)
				{
					m_Instance = go.AddComponent<T>();
					m_Instance.Init();
				}
			}
			return m_Instance;
		}
	}

	public void Init()
	{

	}
}
*/