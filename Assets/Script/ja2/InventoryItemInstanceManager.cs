using System;

namespace ja2
{
	//! Inventory item instance manager.
	/*!
		Create concrete inventory item instances.
	*/
	public class InventoryItemInstanceManager
	{
#region Attributes
		//! Inventory item manager.
		private ja2.InventoryItemManager inventoryItemManager;
#endregion

#region Operations
		//! Create inventory item instance.
		public InventoryItemInstance Create(string Name, sbyte Status)
		{
			ja2.InventoryItemInstance instance;
			// Load inventory as first
			var inventory_item = inventoryItemManager.load(Name);
			// Based on type create instance
			switch (inventory_item._2.type)
			{
				case "torso_clothing":
					instance = new ja2.InventoryItemInstanceTorsoClothing(inventory_item._1, Status, new ja2.TorsoClothing(inventory_item._2.src));
					break;
				case "head_clothing":
					instance = new ja2.InventoryItemInstanceHeadClothing(inventory_item._1, Status, new ja2.HeadClothing(inventory_item._2.src));
					break;
				default:
					throw new Exception("Unknown inventory item type.");
			}

			return instance;
		}
#endregion

#region Construction
		public InventoryItemInstanceManager()
		{
			inventoryItemManager = new InventoryItemManager("Data");
		}
#endregion
	}
} /*ja2*/
