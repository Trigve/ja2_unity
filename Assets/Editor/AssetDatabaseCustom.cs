using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

class AssetDatabaseCustom : ja2.script.IEditor
{
#region Interface
	public string GetAssetPath(UnityEngine.Object AssetObject)
	{
		return AssetDatabase.GetAssetPath(AssetObject);
	}

	public void CreateAsset(UnityEngine.Object Asset, string Path)
	{
		AssetDatabase.CreateAsset(Asset, Path);
	}

	public UnityEngine.Object GetPrefabParent(UnityEngine.Object Instance)
	{
		return PrefabUtility.GetPrefabParent(Instance);
	}
	
	//! PrefabUtility
	public UnityEngine.Object InstantiatePrefab(UnityEngine.Object Target)
	{
		return PrefabUtility.InstantiatePrefab(Target);
	}
#endregion
	
}
