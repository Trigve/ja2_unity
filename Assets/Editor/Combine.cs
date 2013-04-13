using UnityEngine;
using UnityEditor;
using System.Collections;

public class Combine : EditorWindow
{
	[MenuItem("Combine/Create")]
	static void ShowWindow()
	{
		// No GameObject selected
		var obj = (GameObject)Selection.activeObject;
		if (!obj)
			return;

		// Create animator GO
		GameObject animator_go = new GameObject("Animator");
		animator_go.AddComponent<Animator>();

		AssetDatabase.CreateAsset(MeshCombiner.Combine(obj, animator_go), "Assets/Mesh/Combined.asset");
	}
}
