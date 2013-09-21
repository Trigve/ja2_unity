using UnityEngine;
using UnityEditor;

public class CustomScenePlay : MonoBehaviour
{
#region Constants
	public const string DEFAULT_SCENE_KEY = "__default_level";
#endregion
	

	[MenuItem("Play/Play main scene")]
	static void Play()
	{
		EditorApplication.SaveCurrentSceneIfUserWantsTo();
		// Save current scene
		string current_scene = EditorApplication.currentScene;
		EditorPrefs.SetString(DEFAULT_SCENE_KEY, current_scene);
		// Open testing scene
		EditorApplication.OpenScene("Assets/Scenes/2/2.unity");
		EditorApplication.isPlaying = true;
	}
}
