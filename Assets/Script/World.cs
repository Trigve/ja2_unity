using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//! Global class alive for whole game durtion.
/*!
	All global game data goes here.
*/
public sealed class World : MonoSingleton<World>
{
#region Attributes
	//! Inventory item manager.
	private ja2.InventoryItemManager inventoryItemManager;
#endregion

#region Operations
	public override void Init()
	{
		inventoryItemManager = new ja2.InventoryItemManager("Data");
	}

	//! Create inventory item instance.
	public ja2.InventoryItemInstance CreateInventoryItem(string Name, sbyte Status)
	{
		ja2.InventoryItemInstance instance;
		// Load inventory as first
		var inventory_item = inventoryItemManager.load(Name);
		// Based on type create instance
		switch(inventory_item._2.type)
		{
			case "torso_clothing":
				instance = new ja2.InventoryItemInstanceTorsoClothing(inventory_item._1, Status, new ja2.TorsoClothing(inventory_item._2.src));
				break;
			default:
				throw new Exception("Unknown inventory item type.");
		}

		return instance;
	}
#endregion
}
