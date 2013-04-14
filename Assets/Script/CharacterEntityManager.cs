using UnityEngine;
using System.Collections.Generic;

public sealed class CharacterEntityManager
{
#region Attributes
	//! Character definition manager.
	private ja2.CharacterDefinitionManager charDefManager;
	//! Clothes manager.
	private ja2.ClothManager clothesManager;
#endregion

#region Operations
	//! Generate character.
	public void Create(ja2.Character Char, GameObject CharGO)
	{
		// All clothes prefabs
		var clothes_prefabs = new List<GameObject>();
		// Set all parts to defaults
		var char_parts = new Dictionary<ja2.CharacterPart, ja2.CharacterType>()
		{
			{ja2.CharacterPart.Head, ja2.CharacterType.Full},
			{ja2.CharacterPart.Torso, ja2.CharacterType.Full}
		};
		// Get all clothes and compute all parts types
		foreach (var cloth in Char.clothes)
		{
			ja2.ClothItemGroup cloth_item_group = clothesManager.load(cloth, Char.group);
			foreach (var cloth_item in cloth_item_group.items)
				char_parts[cloth_item.part] = cloth_item.type;
			// Add prefab
			clothes_prefabs.Add(PrefabManager.Create(cloth_item_group.prefab));
		}
		// Get body part prefabs
		var body_part_prefabs = new List<GameObject>();
		foreach (var body_part in char_parts)
		{
			body_part_prefabs.Add(PrefabManager.Create(charDefManager.PartPrefab(body_part.Key, Char.group, body_part.Value)));
		}
		// Create temporary object for holding all character parts
		GameObject char_object = new GameObject();
		// Add all parts to temporary object
		foreach (var body_part in body_part_prefabs)
			body_part.transform.parent = char_object.transform;
		// Add all clothes
		foreach (var clothing in clothes_prefabs)
			clothing.transform.parent = char_object.transform;
		// Combine mesh
		var combinded_go = new GameObject("CombinedMesh");
		combinded_go.transform.parent = CharGO.transform;
		MeshCombiner.Combine(char_object, combinded_go);
		// Destroy temporary object
		GameObject.DestroyImmediate(char_object);
	}
#endregion

#region Construction
	public CharacterEntityManager(ja2.CharacterDefinitionManager DefManager, ja2.ClothManager ClothManager)
	{
		charDefManager = DefManager;
		clothesManager = ClothManager;
	}
#endregion
}
