using UnityEngine;
using System.Collections;

namespace utils
{
	public static class PrefabManager
	{
		//! Instantiate prefab.
		static public GameObject Create(string Name)
		{
			var prefab_class = Resources.Load("Prefabs/" + Name, typeof(GameObject));
			GameObject prefab = (GameObject)GameObject.Instantiate(prefab_class);
			prefab.name = prefab_class.name;

			return prefab;
		}
	}
} /*utils*/