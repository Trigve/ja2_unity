using UnityEngine;
using UnityEditor;

public static class PrefabManagerEditor
{
	//! Instantiate prefab.
	static public GameObject Create(string Name)
	{
		var prefab_class = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/" + Name + ".prefab", typeof(GameObject));
		GameObject prefab = (GameObject)GameObject.Instantiate(prefab_class);
		prefab.name = prefab_class.name;

		return prefab;
	}
}
