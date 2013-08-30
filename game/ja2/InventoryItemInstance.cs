namespace ja2
{
	//! Concrete instance of inventory item.
	/*!
		This base class is used as base for all concrete inventory item 
		implementations (torso gear, head gear, weapons, ...).
	*/
	public abstract class InventoryItemInstance
	{
#region Attributes
		//! Inventory item.
		public readonly InventoryItem item;
		//! Health status of item.
		public sbyte status;
#endregion

#region Construction
		public InventoryItemInstance(InventoryItem Item, sbyte Status)
		{
			item = Item;
			status = Status;
		}
#endregion
	}
} /*ja2*/
