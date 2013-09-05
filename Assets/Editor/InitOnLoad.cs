using UnityEngine;
using UnityEditor;
using System.Collections;

//! Handle the editor-on-load stuff and assets modifications.
[InitializeOnLoad]
public class InitOnLoad : UnityEditor.AssetModificationProcessor
{
#region Constants
	//! Temp file name for touching.
	private const string TEMP_FILE_NAME = "__ja2_editor_change";
#endregion
	

#region Operations
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
		// Editor -> Player
		if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
		{
			Debug.Log("Change in EDIT");
			SendSerialize();
			// Touch file to signal we're going to player
			System.IO.File.Create(Application.temporaryCachePath + "/" + TEMP_FILE_NAME);
		}
		// Player -> Editor
		else if(!EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying)
		{
			// Remove file to signal we're going to editor
			System.IO.File.Delete(Application.temporaryCachePath + "/" + TEMP_FILE_NAME);
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
#endregion

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
		try
		{
			// Only if not in player, otherwise double deserialization occurs
			// (one here, another one in Awake() of serialization component)
			if(!System.IO.File.Exists(Application.temporaryCachePath + "/" + TEMP_FILE_NAME))
				SendDeSerialize();
		}
		catch(System.Exception e)
		{
			Debug.LogWarning("Exception while deserializing on init:" + e.ToString());
		}
	}
#endregion
}
