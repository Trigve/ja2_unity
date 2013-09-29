using UnityEngine;
using UnityEditor;

public static class PrefabManagerEditor
{
	//! Instantiate prefab.
	static public GameObject Create(string Name)
	{
		var prefab_class = AssetDatabase.LoadAssetAtPath("Assets/" + Name + ".prefab", typeof(GameObject));
		GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab_class);
		prefab.name = prefab_class.name;

		return prefab;
	}

	//! Instantiate resource prefab.
	static public GameObject CreateFromResources(string Name)
	{
		var prefab_class = Resources.Load("Prefabs/" + Name, typeof(GameObject));
		GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab_class);
		prefab.name = prefab_class.name;

		return prefab;
	}
}
