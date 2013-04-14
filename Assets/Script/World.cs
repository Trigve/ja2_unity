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
	private ja2.InventoryItemInstanceManager inventoryInstanceManager;
#endregion

#region Operations
	public override void Init()
	{
		inventoryInstanceManager = new ja2.InventoryItemInstanceManager();
	}

	//! Create inventory item instance.
	public ja2.InventoryItemInstance CreateInventoryItem(string Name, sbyte Status)
	{
		return inventoryInstanceManager.Create(Name, Status);
	}
#endregion
}
