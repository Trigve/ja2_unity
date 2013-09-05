using UnityEngine;
using UnityEditor;
using System.Collections;

//! Handle the editor-on-load stuff and assets modifications.
[InitializeOnLoad]
public class InitOnLoad : UnityEditor.AssetModificationProcessor
{
	private static bool isEdit = true;

	//! Hande pre-save action.
	public static string[] OnWillSaveAssets(string[] Paths)
	{
		// Try to serialize objects
		SendSerialize();

		return Paths;
	}

	//! Handle the editor -> player change.
	public static void Change()
	{
		if (EditorApplication.isPlaying || EditorApplication.isPaused)
		{
			if (isEdit)
			{
				isEdit = false;
			}
		}
		else
		{
			if (!isEdit)
			{
				isEdit = true;
			}
			// This is important, we're in editor and are begin to play
			else
			{
				Debug.Log("Change in EDIT");
				SendSerialize();				
			}
		}
	}

	//! Try to serialize all game objects.
	static void SendSerialize()
	{
		Object[] gos = GameObject.FindObjectsOfType(typeof(GameObject));
		foreach (GameObject go in gos)
		{
			var serialize_component = go.GetComponent<SerializableComponent>();
			if (serialize_component != null)
				serialize_component.Serialize();
		}
	}

	//! Try to serialize all game objects.
	static void SendDeSerialize()
	{
		Object[] gos = GameObject.FindObjectsOfType(typeof(GameObject));
		foreach (GameObject go in gos)
		{
			var serialize_component = go.GetComponent<SerializableComponent>();
			if (serialize_component != null)
				serialize_component.Deserialize();
		}
	}

#region Construction
	static InitOnLoad()
	{
		Debug.Log("InitOnLoad loaded...");
		EditorApplication.playmodeStateChanged += Change;
		// InitOnLoad() is ran not only on editor startup but also if we're in
		// editor and some code needs to be reloaded. Therefor, all
		// components in editor are default-serialized by unity, without
		// executing our serialization code. We must the explicitly call
		// deserializatin code.
		SendDeSerialize();
	}
#endregion
}
